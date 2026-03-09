using Avalonia.Controls.Maui.Essentials;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaPreferencesTests : IDisposable
{
    readonly AvaloniaPreferences _preferences;

    public AvaloniaPreferencesTests()
    {
        _preferences = new AvaloniaPreferences();
        _preferences.Clear();
    }

    public void Dispose()
    {
        _preferences.Clear();
    }

    [Fact]
    public void Set_And_Get_String()
    {
        _preferences.Set("key", "hello");
        Assert.Equal("hello", _preferences.Get("key", "default"));
    }

    [Fact]
    public void Set_And_Get_Int()
    {
        _preferences.Set("key", 42);
        Assert.Equal(42, _preferences.Get("key", 0));
    }

    [Fact]
    public void Set_And_Get_Bool_True()
    {
        _preferences.Set("key", true);
        Assert.True(_preferences.Get("key", false));
    }

    [Fact]
    public void Set_And_Get_Bool_False()
    {
        _preferences.Set("key", false);
        Assert.False(_preferences.Get("key", true));
    }

    [Fact]
    public void Set_And_Get_Long()
    {
        _preferences.Set("key", long.MaxValue);
        Assert.Equal(long.MaxValue, _preferences.Get("key", 0L));
    }

    [Fact]
    public void Set_And_Get_Double()
    {
        _preferences.Set("key", 3.14);
        Assert.Equal(3.14, _preferences.Get("key", 0.0));
    }

    [Fact]
    public void Set_And_Get_Float()
    {
        _preferences.Set("key", 2.5f);
        Assert.Equal(2.5f, _preferences.Get("key", 0f));
    }

    [Fact]
    public void Set_And_Get_DateTime()
    {
        var dt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        _preferences.Set("key", dt);
        Assert.Equal(dt, _preferences.Get("key", DateTime.MinValue));
    }

    [Fact]
    public void Set_And_Get_DateTimeOffset()
    {
        var dto = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.FromHours(-5));
        _preferences.Set("key", dto);
        Assert.Equal(dto, _preferences.Get("key", DateTimeOffset.MinValue));
    }

    [Fact]
    public void Get_Missing_Key_Returns_Default()
    {
        Assert.Equal("fallback", _preferences.Get("nonexistent", "fallback"));
        Assert.Equal(99, _preferences.Get("nonexistent", 99));
        Assert.True(_preferences.Get("nonexistent", true));
    }

    [Fact]
    public void ContainsKey_Returns_True_For_Existing_Key()
    {
        _preferences.Set("exists", "value");
        Assert.True(_preferences.ContainsKey("exists"));
    }

    [Fact]
    public void ContainsKey_Returns_False_For_Missing_Key()
    {
        Assert.False(_preferences.ContainsKey("missing"));
    }

    [Fact]
    public void Remove_Deletes_Key()
    {
        _preferences.Set("key", "value");
        Assert.True(_preferences.ContainsKey("key"));

        _preferences.Remove("key");
        Assert.False(_preferences.ContainsKey("key"));
    }

    [Fact]
    public void Remove_NonExistent_Key_Does_Not_Throw()
    {
        _preferences.Remove("nonexistent");
    }

    [Fact]
    public void Clear_Removes_All_Keys()
    {
        _preferences.Set("key1", "a");
        _preferences.Set("key2", "b");
        _preferences.Set("key3", "c");

        _preferences.Clear();

        Assert.False(_preferences.ContainsKey("key1"));
        Assert.False(_preferences.ContainsKey("key2"));
        Assert.False(_preferences.ContainsKey("key3"));
    }

    [Fact]
    public void Set_Null_String_Removes_Key()
    {
        _preferences.Set("key", "value");
        Assert.True(_preferences.ContainsKey("key"));

        _preferences.Set<string?>("key", null);
        Assert.False(_preferences.ContainsKey("key"));
    }

    [Fact]
    public void Set_Overwrites_Previous_Value()
    {
        _preferences.Set("key", "first");
        _preferences.Set("key", "second");
        Assert.Equal("second", _preferences.Get("key", "default"));
    }

    [Fact]
    public void SharedName_Isolates_Keys()
    {
        _preferences.Set("key", "default_value");
        _preferences.Set("key", "shared_value", "myshare");

        Assert.Equal("default_value", _preferences.Get("key", ""));
        Assert.Equal("shared_value", _preferences.Get("key", "", "myshare"));
    }

    [Fact]
    public void ContainsKey_With_SharedName()
    {
        _preferences.Set("key", "value", "share1");

        Assert.False(_preferences.ContainsKey("key"));
        Assert.True(_preferences.ContainsKey("key", "share1"));
    }

    [Fact]
    public void Remove_With_SharedName()
    {
        _preferences.Set("key", "value", "share1");
        _preferences.Set("key", "value2");

        _preferences.Remove("key", "share1");

        Assert.False(_preferences.ContainsKey("key", "share1"));
        Assert.True(_preferences.ContainsKey("key"));
    }

    [Fact]
    public void Clear_With_SharedName_Only_Clears_That_Container()
    {
        _preferences.Set("key", "default_val");
        _preferences.Set("key", "shared_val", "myshare");

        _preferences.Clear("myshare");

        Assert.True(_preferences.ContainsKey("key"));
        Assert.False(_preferences.ContainsKey("key", "myshare"));
    }

    [Fact]
    public void Clear_Without_SharedName_Does_Not_Clear_Other_Containers()
    {
        _preferences.Set("key", "default_val");
        _preferences.Set("key", "shared_val", "myshare");

        _preferences.Clear();

        Assert.False(_preferences.ContainsKey("key"));
        Assert.True(_preferences.ContainsKey("key", "myshare"));
    }

    [Fact]
    public void Persistence_Survives_New_Instance()
    {
        _preferences.Set("persist_key", "persist_value");

        var newInstance = new AvaloniaPreferences();
        Assert.Equal("persist_value", newInstance.Get("persist_key", "missing"));
    }

    [Fact]
    public void ContainsKey_Returns_False_For_Unknown_SharedName()
    {
        _preferences.Set("key", "value");
        Assert.False(_preferences.ContainsKey("key", "unknown_share"));
    }

    [Fact]
    public void Set_And_Get_Negative_Int()
    {
        _preferences.Set("key", -100);
        Assert.Equal(-100, _preferences.Get("key", 0));
    }

    [Fact]
    public void Set_And_Get_Empty_String()
    {
        _preferences.Set("key", "");
        Assert.Equal("", _preferences.Get("key", "default"));
    }

    [Fact]
    public void Multiple_Keys_Coexist()
    {
        _preferences.Set("str", "text");
        _preferences.Set("num", 42);
        _preferences.Set("flag", true);

        Assert.Equal("text", _preferences.Get("str", ""));
        Assert.Equal(42, _preferences.Get("num", 0));
        Assert.True(_preferences.Get("flag", false));
    }
}
