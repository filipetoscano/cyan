namespace LargeXlsx;

/// <summary />
public class XlsxStyles
{
    /// <summary />
    public static XlsxStyle Text = new XlsxStyle( XlsxFont.Default, XlsxFill.None, XlsxBorder.None, XlsxNumberFormat.Text, XlsxAlignment.Default );

    /// <summary />
    public static XlsxStyle Date = new XlsxStyle( XlsxFont.Default, XlsxFill.None, XlsxBorder.None, XlsxNumberFormat.ShortDate, XlsxAlignment.Default );

    /// <summary />
    public static XlsxStyle Money = new XlsxStyle( XlsxFont.Default, XlsxFill.None, XlsxBorder.None, XlsxNumberFormat.TwoDecimal, XlsxAlignment.Default );
}