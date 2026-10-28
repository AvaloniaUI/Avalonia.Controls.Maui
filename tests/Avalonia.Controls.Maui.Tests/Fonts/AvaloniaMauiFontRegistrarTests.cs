using System.Buffers.Binary;
using System.Text;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;

namespace Avalonia.Controls.Maui.Tests.Fonts;

public class AvaloniaMauiFontRegistrarTests
{
    const ushort UnicodePlatform = 0;
    const ushort MacintoshPlatform = 1;
    const ushort WindowsPlatform = 3;
    const ushort WindowsUnicodeBmp = 1;
    const ushort USEnglish = 0x0409;
    const ushort German = 0x0407;

    [Fact(DisplayName = "Family name stored for the Unicode platform is decoded as UTF-16")]
    public void FamilyNameStoredForUnicodePlatformIsDecodedAsUtf16()
    {
        // The record layout of diablo_h.ttf
        var font = CreateFont(
            (UnicodePlatform, 0, 0, Encoding.BigEndianUnicode.GetBytes("Diablo")),
            (MacintoshPlatform, 0, 0, Encoding.ASCII.GetBytes("Diablo")),
            (WindowsPlatform, WindowsUnicodeBmp, USEnglish, Encoding.BigEndianUnicode.GetBytes("Diablo")));

        Assert.Equal("Diablo", GetFontFamilyName(font));
    }

    [Fact(DisplayName = "Windows US English family name is preferred over earlier records")]
    public void WindowsUSEnglishFamilyNameIsPreferredOverEarlierRecords()
    {
        // The record layout of archristy.ttf
        var font = CreateFont(
            (UnicodePlatform, 0, 0, Encoding.BigEndianUnicode.GetBytes("CHRISTY")),
            (MacintoshPlatform, 0, 0, Encoding.ASCII.GetBytes("AR CHRISTY")),
            (WindowsPlatform, WindowsUnicodeBmp, USEnglish, Encoding.BigEndianUnicode.GetBytes("AR CHRISTY")));

        Assert.Equal("AR CHRISTY", GetFontFamilyName(font));
    }

    [Fact(DisplayName = "Windows US English family name is preferred over other languages")]
    public void WindowsUSEnglishFamilyNameIsPreferredOverOtherLanguages()
    {
        var font = CreateFont(
            (WindowsPlatform, WindowsUnicodeBmp, German, Encoding.BigEndianUnicode.GetBytes("Schrift")),
            (WindowsPlatform, WindowsUnicodeBmp, USEnglish, Encoding.BigEndianUnicode.GetBytes("Font")));

        Assert.Equal("Font", GetFontFamilyName(font));
    }

    [Fact(DisplayName = "First Windows family name is used without a US English one")]
    public void FirstWindowsFamilyNameIsUsedWithoutUSEnglish()
    {
        var font = CreateFont(
            (MacintoshPlatform, 0, 0, Encoding.ASCII.GetBytes("Mac Name")),
            (WindowsPlatform, WindowsUnicodeBmp, German, Encoding.BigEndianUnicode.GetBytes("Schrift")));

        Assert.Equal("Schrift", GetFontFamilyName(font));
    }

    [Fact(DisplayName = "Macintosh family name is used when it is the only one")]
    public void MacintoshFamilyNameIsUsedWhenItIsTheOnlyOne()
    {
        var font = CreateFont(
            (MacintoshPlatform, 0, 0, Encoding.ASCII.GetBytes("Diablo")));

        Assert.Equal("Diablo", GetFontFamilyName(font));
    }

    [AvaloniaTheory(DisplayName = "Family name matches the one Avalonia reads from the font")]
    [InlineData("GalleryScript-Regular.ttf")]
    [InlineData("OpenSans-Regular.ttf")]
    public void FamilyNameMatchesTheOneAvaloniaReads(string fileName)
    {
        var avaloniaName = new TestFontCollection().Load(OpenTestFont(fileName))?.FamilyName;
        var registrarName = new AvaloniaMauiFontRegistrar().GetFontFamilyName(OpenTestFont(fileName));

        Assert.False(string.IsNullOrEmpty(avaloniaName));
        Assert.Equal(avaloniaName, registrarName);
    }

    [AvaloniaFact(DisplayName = "Registered font alias resolves to a font Avalonia loads")]
    public void RegisteredFontAliasResolvesToFontAvaloniaLoads()
    {
        var registrar = new AvaloniaMauiFontRegistrar();
        registrar.Register("GalleryScript-Regular.ttf", "GalleryScript", typeof(AvaloniaMauiFontRegistrarTests).Assembly);

        var font = registrar.GetFont("GalleryScript");

        Assert.NotNull(font);
        Assert.True(Avalonia.Media.FontManager.Current.TryGetGlyphTypeface(new Typeface(new FontFamily(font!)), out var glyphTypeface));
        Assert.Equal("Gallery Script", glyphTypeface!.FamilyName);
    }

    static Stream OpenTestFont(string fileName) =>
        AssetLoader.Open(new Uri($"avares://{typeof(AvaloniaMauiFontRegistrarTests).Assembly.GetName().Name}/Assets/Fonts/{fileName}"));

    sealed class TestFontCollection : FontCollectionBase
    {
        public override Uri Key { get; } = new("fonts:AvaloniaMauiFontRegistrarTests");

        public GlyphTypeface? Load(Stream stream) => TryAddGlyphTypeface(stream, out var glyphTypeface) ? glyphTypeface : null;
    }

    static string? GetFontFamilyName(byte[] font) =>
        new AvaloniaMauiFontRegistrar().GetFontFamilyName(new MemoryStream(font));

    /// <summary>
    /// Builds a font file that holds only a 'name' table with font family name records.
    /// </summary>
    static byte[] CreateFont(params (ushort Platform, ushort Encoding, ushort Language, byte[] Name)[] records)
    {
        const int headerLength = 12 + 16;
        var storageOffset = 6 + 12 * records.Length;
        var storageLength = records.Sum(r => r.Name.Length);
        var font = new byte[headerLength + storageOffset + storageLength];
        var span = font.AsSpan();

        // Offset table with a single 'name' table record
        BinaryPrimitives.WriteUInt32BigEndian(span[0..], 0x00010000);
        BinaryPrimitives.WriteUInt16BigEndian(span[4..], 1);
        Encoding.ASCII.GetBytes("name").CopyTo(span[12..]);
        BinaryPrimitives.WriteUInt32BigEndian(span[20..], headerLength);
        BinaryPrimitives.WriteUInt32BigEndian(span[24..], (uint)(storageOffset + storageLength));

        // 'name' table: format 0, record count, string storage offset, then the records
        var table = span[headerLength..];
        BinaryPrimitives.WriteUInt16BigEndian(table[2..], (ushort)records.Length);
        BinaryPrimitives.WriteUInt16BigEndian(table[4..], (ushort)storageOffset);

        var stringOffset = 0;
        for (var i = 0; i < records.Length; i++)
        {
            var (platform, encoding, language, name) = records[i];
            var record = table[(6 + 12 * i)..];
            BinaryPrimitives.WriteUInt16BigEndian(record[0..], platform);
            BinaryPrimitives.WriteUInt16BigEndian(record[2..], encoding);
            BinaryPrimitives.WriteUInt16BigEndian(record[4..], language);
            BinaryPrimitives.WriteUInt16BigEndian(record[6..], 1);
            BinaryPrimitives.WriteUInt16BigEndian(record[8..], (ushort)name.Length);
            BinaryPrimitives.WriteUInt16BigEndian(record[10..], (ushort)stringOffset);
            name.CopyTo(table[(storageOffset + stringOffset)..]);
            stringOffset += name.Length;
        }

        return font;
    }
}
