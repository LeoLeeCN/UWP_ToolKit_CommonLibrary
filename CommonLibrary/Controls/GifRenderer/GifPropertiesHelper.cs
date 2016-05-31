using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;

namespace CommonLibrary
{
    public class GifPropertiesHelper
    {   
        /// <summary>
         /// Retrieve frame specific properties. Each frame has an individual delay before the next, as well as top & left from where the first change appears in the bytes.
         /// </summary>
        public async Task<FrameProperties> RetrieveFramePropertiesAsync(BitmapFrame frame, int index)
        {
            const string leftProperty = "/imgdesc/Left";
            const string topProperty = "/imgdesc/Top";
            const string widthProperty = "/imgdesc/Width";
            const string heightProperty = "/imgdesc/Height";
            const string delayProperty = "/grctlext/Delay";
            const string disposalProperty = "/grctlext/Disposal";

            var propertiesView = frame.BitmapProperties;
            var requiredProperties = new[] { leftProperty, topProperty, widthProperty, heightProperty };
            var properties = await propertiesView.GetPropertiesAsync(requiredProperties);

            var left = (ushort)properties[leftProperty].Value;
            var top = (ushort)properties[topProperty].Value;
            var width = (ushort)properties[widthProperty].Value;
            var height = (ushort)properties[heightProperty].Value;

            var delayMilliseconds = 30.0;
            var shouldDispose = false;

            try
            {
                var extensionProperties = new[] { delayProperty, disposalProperty };
                properties = await propertiesView.GetPropertiesAsync(extensionProperties);

                if (properties.ContainsKey(delayProperty) && properties[delayProperty].Type == PropertyType.UInt16)
                {
                    var delayInHundredths = (ushort)properties[delayProperty].Value;
                    if (delayInHundredths >= 3u) // Prevent degenerate frames with no delay time
                    {
                        delayMilliseconds = 10.0 * delayInHundredths;
                    }
                }

                if (properties.ContainsKey(disposalProperty) && properties[disposalProperty].Type == PropertyType.UInt8)
                {
                    var disposal = (byte)properties[disposalProperty].Value;
                    if (disposal == 2)
                    {
                        shouldDispose = true;
                    }
                }
            }
            catch
            {
            }

            return new FrameProperties(
                new Rect(left, top, width, height),
                delayMilliseconds,
                shouldDispose,
                index);
        }

        /// <summary>
        /// The entire gif image has properties such as width and height thats required and has to be used in calculating bytes. BitmapDecoder comes with width and height, but those are inaccurate compared to these properties.
        /// </summary>
        public async Task<ImageProperties> RetrieveImagePropertiesAsync(BitmapDecoder bitmapDecoder)
        {
            // Properties not currently supported: background color, pixel aspect ratio.
            const string widthProperty = "/logscrdesc/Width";
            const string heightProperty = "/logscrdesc/Height";
            const string applicationProperty = "/appext/application";
            const string dataProperty = "/appext/data";

            var propertiesView = bitmapDecoder.BitmapContainerProperties;
            var requiredProperties = new[] { widthProperty, heightProperty };
            var properties = await propertiesView.GetPropertiesAsync(requiredProperties);

            var pixelWidth = (ushort)properties[widthProperty].Value;
            var pixelHeight = (ushort)properties[heightProperty].Value;

            var loopCount = 0;
            var isAnimated = true;

            try
            {
                var extensionProperties = new[] { applicationProperty, dataProperty };
                properties = await propertiesView.GetPropertiesAsync(extensionProperties);

                if (properties.ContainsKey(applicationProperty) &&
                    properties[applicationProperty].Type == PropertyType.UInt8Array)
                {
                    var bytes = (byte[])properties[applicationProperty].Value;
                    var applicationName = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    if (applicationName == "NETSCAPE2.0" || applicationName == "ANIMEXTS1.0")
                    {
                        if (properties.ContainsKey(dataProperty) && properties[dataProperty].Type == PropertyType.UInt8Array)
                        {
                            var data = (byte[])properties[dataProperty].Value;
                            loopCount = data[2] | data[3] << 8;
                            isAnimated = data[1] == 1;
                        }
                    }
                }
            }
            catch
            {
            }

            return new ImageProperties(pixelWidth, pixelHeight, isAnimated, loopCount);
        }
    }
}
