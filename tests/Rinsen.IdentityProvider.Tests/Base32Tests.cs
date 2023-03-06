using Xunit;

namespace Rinsen.IdentityProvider.Tests;

public class Base32Tests
{

    [Fact]
    public void WhenBase32EncodingByteArray_GetCorrectStringBack()
    {
        Assert.Equal("", Base32.Encode(new byte[] { })); // ""
        Assert.Equal("MY======", Base32.Encode(new byte[] { 0x66 })); // f
        Assert.Equal("MZXQ====", Base32.Encode(new byte[] { 0x66, 0x6F })); // fo
        Assert.Equal("MZXW6===", Base32.Encode(new byte[] { 0x66, 0x6F, 0x6F })); // foo
        Assert.Equal("MZXW6YQ=", Base32.Encode(new byte[] { 0x66, 0x6F, 0x6F, 0x62 })); // foo
        Assert.Equal("MZXW6YTB", Base32.Encode(new byte[] { 0x66, 0x6F, 0x6F, 0x62, 0x61 })); // fooba
        Assert.Equal("MZXW6YTBOI======", Base32.Encode(new byte[] { 0x66, 0x6F, 0x6F, 0x62, 0x61, 0x72 })); // foobar
    }


}
