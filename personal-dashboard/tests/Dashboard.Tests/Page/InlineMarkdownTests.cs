using PersonalDashboard.Page;

namespace PersonalDashboard.Tests.Page;

public class InlineMarkdownTests
{
    [Fact]
    public void Turns_backticks_into_code()
    {
        Assert.Equal("Run <code>make</code> now", InlineMarkdown.ToHtml("Run `make` now"));
    }

    [Fact]
    public void Turns_double_asterisks_into_bold()
    {
        Assert.Equal("Book the <strong>plumber</strong>", InlineMarkdown.ToHtml("Book the **plumber**"));
    }

    [Fact]
    public void Encodes_html_before_applying_markup()
    {
        Assert.Equal("&lt;script&gt;x&lt;/script&gt; &amp; <strong>&lt;b&gt;</strong>", InlineMarkdown.ToHtml("<script>x</script> & **<b>**"));
    }

    [Fact]
    public void Bold_markers_inside_code_stay_literal()
    {
        Assert.Equal("<code>**x**</code>", InlineMarkdown.ToHtml("`**x**`"));
    }

    [Fact]
    public void Encodes_quotes()
    {
        Assert.Equal("&quot;a&quot; &#39;b&#39;", InlineMarkdown.ToHtml("\"a\" 'b'"));
    }
}
