using System.Collections.Generic;
using System;
using System.Linq;
using System.Drawing;

namespace Silmoon.AspNetCore.Html
{
    public class HtmlColor
    {
        public static HtmlColor AliceBlue => new HtmlColor(ColorNameMap["aliceblue"]);
        public static HtmlColor AntiqueWhite => new HtmlColor(ColorNameMap["antiquewhite"]);
        public static HtmlColor Aqua => new HtmlColor(ColorNameMap["aqua"]);
        public static HtmlColor Aquamarine => new HtmlColor(ColorNameMap["aquamarine"]);
        public static HtmlColor Azure => new HtmlColor(ColorNameMap["azure"]);
        public static HtmlColor Beige => new HtmlColor(ColorNameMap["beige"]);
        public static HtmlColor Bisque => new HtmlColor(ColorNameMap["bisque"]);
        public static HtmlColor Black => new HtmlColor(ColorNameMap["black"]);
        public static HtmlColor BlanchedAlmond => new HtmlColor(ColorNameMap["blanchedalmond"]);
        public static HtmlColor Blue => new HtmlColor(ColorNameMap["blue"]);
        public static HtmlColor BlueViolet => new HtmlColor(ColorNameMap["blueviolet"]);
        public static HtmlColor Brown => new HtmlColor(ColorNameMap["brown"]);
        public static HtmlColor BurlyWood => new HtmlColor(ColorNameMap["burlywood"]);
        public static HtmlColor CadetBlue => new HtmlColor(ColorNameMap["cadetblue"]);
        public static HtmlColor Chartreuse => new HtmlColor(ColorNameMap["chartreuse"]);
        public static HtmlColor Chocolate => new HtmlColor(ColorNameMap["chocolate"]);
        public static HtmlColor Coral => new HtmlColor(ColorNameMap["coral"]);
        public static HtmlColor CornflowerBlue => new HtmlColor(ColorNameMap["cornflowerblue"]);
        public static HtmlColor Cornsilk => new HtmlColor(ColorNameMap["cornsilk"]);
        public static HtmlColor Crimson => new HtmlColor(ColorNameMap["crimson"]);
        public static HtmlColor Cyan => new HtmlColor(ColorNameMap["cyan"]);
        public static HtmlColor DarkBlue => new HtmlColor(ColorNameMap["darkblue"]);
        public static HtmlColor DarkCyan => new HtmlColor(ColorNameMap["darkcyan"]);
        public static HtmlColor DarkGoldenRod => new HtmlColor(ColorNameMap["darkgoldenrod"]);
        public static HtmlColor DarkGray => new HtmlColor(ColorNameMap["darkgray"]);
        public static HtmlColor DarkGreen => new HtmlColor(ColorNameMap["darkgreen"]);
        public static HtmlColor DarkGrey => new HtmlColor(ColorNameMap["darkgrey"]);
        public static HtmlColor DarkKhaki => new HtmlColor(ColorNameMap["darkkhaki"]);
        public static HtmlColor DarkMagenta => new HtmlColor(ColorNameMap["darkmagenta"]);
        public static HtmlColor DarkOliveGreen => new HtmlColor(ColorNameMap["darkolivegreen"]);
        public static HtmlColor DarkOrange => new HtmlColor(ColorNameMap["darkorange"]);
        public static HtmlColor DarkOrchid => new HtmlColor(ColorNameMap["darkorchid"]);
        public static HtmlColor DarkRed => new HtmlColor(ColorNameMap["darkred"]);
        public static HtmlColor DarkSalmon => new HtmlColor(ColorNameMap["darksalmon"]);
        public static HtmlColor DarkSeaGreen => new HtmlColor(ColorNameMap["darkseagreen"]);
        public static HtmlColor DarkSlateBlue => new HtmlColor(ColorNameMap["darkslateblue"]);
        public static HtmlColor DarkSlateGray => new HtmlColor(ColorNameMap["darkslategray"]);
        public static HtmlColor DarkSlateGrey => new HtmlColor(ColorNameMap["darkslategrey"]);
        public static HtmlColor DarkTurquoise => new HtmlColor(ColorNameMap["darkturquoise"]);
        public static HtmlColor DarkViolet => new HtmlColor(ColorNameMap["darkviolet"]);
        public static HtmlColor DeepPink => new HtmlColor(ColorNameMap["deeppink"]);
        public static HtmlColor DeepSkyBlue => new HtmlColor(ColorNameMap["deepskyblue"]);
        public static HtmlColor DimGray => new HtmlColor(ColorNameMap["dimgray"]);
        public static HtmlColor DimGrey => new HtmlColor(ColorNameMap["dimgrey"]);
        public static HtmlColor DodgerBlue => new HtmlColor(ColorNameMap["dodgerblue"]);
        public static HtmlColor FireBrick => new HtmlColor(ColorNameMap["firebrick"]);
        public static HtmlColor FloralWhite => new HtmlColor(ColorNameMap["floralwhite"]);
        public static HtmlColor ForestGreen => new HtmlColor(ColorNameMap["forestgreen"]);
        public static HtmlColor Fuchsia => new HtmlColor(ColorNameMap["fuchsia"]);
        public static HtmlColor Gainsboro => new HtmlColor(ColorNameMap["gainsboro"]);
        public static HtmlColor GhostWhite => new HtmlColor(ColorNameMap["ghostwhite"]);
        public static HtmlColor Gold => new HtmlColor(ColorNameMap["gold"]);
        public static HtmlColor GoldenRod => new HtmlColor(ColorNameMap["goldenrod"]);
        public static HtmlColor Gray => new HtmlColor(ColorNameMap["gray"]);
        public static HtmlColor Green => new HtmlColor(ColorNameMap["green"]);
        public static HtmlColor GreenYellow => new HtmlColor(ColorNameMap["greenyellow"]);
        public static HtmlColor Grey => new HtmlColor(ColorNameMap["grey"]);
        public static HtmlColor HoneyDew => new HtmlColor(ColorNameMap["honeydew"]);
        public static HtmlColor HotPink => new HtmlColor(ColorNameMap["hotpink"]);
        public static HtmlColor IndianRed => new HtmlColor(ColorNameMap["indianred"]);
        public static HtmlColor Indigo => new HtmlColor(ColorNameMap["indigo"]);
        public static HtmlColor Ivory => new HtmlColor(ColorNameMap["ivory"]);
        public static HtmlColor Khaki => new HtmlColor(ColorNameMap["khaki"]);
        public static HtmlColor Lavender => new HtmlColor(ColorNameMap["lavender"]);
        public static HtmlColor LavenderBlush => new HtmlColor(ColorNameMap["lavenderblush"]);
        public static HtmlColor LawnGreen => new HtmlColor(ColorNameMap["lawngreen"]);
        public static HtmlColor LemonChiffon => new HtmlColor(ColorNameMap["lemonchiffon"]);
        public static HtmlColor LightBlue => new HtmlColor(ColorNameMap["lightblue"]);
        public static HtmlColor LightCoral => new HtmlColor(ColorNameMap["lightcoral"]);
        public static HtmlColor LightCyan => new HtmlColor(ColorNameMap["lightcyan"]);
        public static HtmlColor LightGoldenRodYellow => new HtmlColor(ColorNameMap["lightgoldenrodyellow"]);
        public static HtmlColor LightGray => new HtmlColor(ColorNameMap["lightgray"]);
        public static HtmlColor LightGreen => new HtmlColor(ColorNameMap["lightgreen"]);
        public static HtmlColor LightGrey => new HtmlColor(ColorNameMap["lightgrey"]);
        public static HtmlColor LightPink => new HtmlColor(ColorNameMap["lightpink"]);
        public static HtmlColor LightSalmon => new HtmlColor(ColorNameMap["lightsalmon"]);
        public static HtmlColor LightSeaGreen => new HtmlColor(ColorNameMap["lightseagreen"]);
        public static HtmlColor LightSkyBlue => new HtmlColor(ColorNameMap["lightskyblue"]);
        public static HtmlColor LightSlateGray => new HtmlColor(ColorNameMap["lightslategray"]);
        public static HtmlColor LightSlateGrey => new HtmlColor(ColorNameMap["lightslategrey"]);
        public static HtmlColor LightSteelBlue => new HtmlColor(ColorNameMap["lightsteelblue"]);
        public static HtmlColor LightYellow => new HtmlColor(ColorNameMap["lightyellow"]);
        public static HtmlColor Lime => new HtmlColor(ColorNameMap["lime"]);
        public static HtmlColor LimeGreen => new HtmlColor(ColorNameMap["limegreen"]);
        public static HtmlColor Linen => new HtmlColor(ColorNameMap["linen"]);
        public static HtmlColor Magenta => new HtmlColor(ColorNameMap["magenta"]);
        public static HtmlColor Maroon => new HtmlColor(ColorNameMap["maroon"]);
        public static HtmlColor MediumAquaMarine => new HtmlColor(ColorNameMap["mediumaquamarine"]);
        public static HtmlColor MediumBlue => new HtmlColor(ColorNameMap["mediumblue"]);
        public static HtmlColor MediumOrchid => new HtmlColor(ColorNameMap["mediumorchid"]);
        public static HtmlColor MediumPurple => new HtmlColor(ColorNameMap["mediumpurple"]);
        public static HtmlColor MediumSeaGreen => new HtmlColor(ColorNameMap["mediumseagreen"]);
        public static HtmlColor MediumSlateBlue => new HtmlColor(ColorNameMap["mediumslateblue"]);
        public static HtmlColor MediumSpringGreen => new HtmlColor(ColorNameMap["mediumspringgreen"]);
        public static HtmlColor MediumTurquoise => new HtmlColor(ColorNameMap["mediumturquoise"]);
        public static HtmlColor MediumVioletRed => new HtmlColor(ColorNameMap["mediumvioletred"]);
        public static HtmlColor MidnightBlue => new HtmlColor(ColorNameMap["midnightblue"]);
        public static HtmlColor MintCream => new HtmlColor(ColorNameMap["mintcream"]);
        public static HtmlColor MistyRose => new HtmlColor(ColorNameMap["mistyrose"]);
        public static HtmlColor Moccasin => new HtmlColor(ColorNameMap["moccasin"]);
        public static HtmlColor NavajoWhite => new HtmlColor(ColorNameMap["navajowhite"]);
        public static HtmlColor Navy => new HtmlColor(ColorNameMap["navy"]);
        public static HtmlColor OldLace => new HtmlColor(ColorNameMap["oldlace"]);
        public static HtmlColor Olive => new HtmlColor(ColorNameMap["olive"]);
        public static HtmlColor OliveDrab => new HtmlColor(ColorNameMap["olivedrab"]);
        public static HtmlColor Orange => new HtmlColor(ColorNameMap["orange"]);
        public static HtmlColor OrangeRed => new HtmlColor(ColorNameMap["orangered"]);
        public static HtmlColor Orchid => new HtmlColor(ColorNameMap["orchid"]);
        public static HtmlColor PaleGoldenRod => new HtmlColor(ColorNameMap["palegoldenrod"]);
        public static HtmlColor PaleGreen => new HtmlColor(ColorNameMap["palegreen"]);
        public static HtmlColor PaleTurquoise => new HtmlColor(ColorNameMap["paleturquoise"]);
        public static HtmlColor PaleVioletRed => new HtmlColor(ColorNameMap["palevioletred"]);
        public static HtmlColor PapayaWhip => new HtmlColor(ColorNameMap["papayawhip"]);
        public static HtmlColor PeachPuff => new HtmlColor(ColorNameMap["peachpuff"]);
        public static HtmlColor Peru => new HtmlColor(ColorNameMap["peru"]);
        public static HtmlColor Pink => new HtmlColor(ColorNameMap["pink"]);
        public static HtmlColor Plum => new HtmlColor(ColorNameMap["plum"]);
        public static HtmlColor PowderBlue => new HtmlColor(ColorNameMap["powderblue"]);
        public static HtmlColor Purple => new HtmlColor(ColorNameMap["purple"]);
        public static HtmlColor RebeccaPurple => new HtmlColor(ColorNameMap["rebeccapurple"]);
        public static HtmlColor Red => new HtmlColor(ColorNameMap["red"]);
        public static HtmlColor RosyBrown => new HtmlColor(ColorNameMap["rosybrown"]);
        public static HtmlColor RoyalBlue => new HtmlColor(ColorNameMap["royalblue"]);
        public static HtmlColor SaddleBrown => new HtmlColor(ColorNameMap["saddlebrown"]);
        public static HtmlColor Salmon => new HtmlColor(ColorNameMap["salmon"]);
        public static HtmlColor SandyBrown => new HtmlColor(ColorNameMap["sandybrown"]);
        public static HtmlColor SeaGreen => new HtmlColor(ColorNameMap["seagreen"]);
        public static HtmlColor SeaShell => new HtmlColor(ColorNameMap["seashell"]);
        public static HtmlColor Sienna => new HtmlColor(ColorNameMap["sienna"]);
        public static HtmlColor Silver => new HtmlColor(ColorNameMap["silver"]);
        public static HtmlColor SkyBlue => new HtmlColor(ColorNameMap["skyblue"]);
        public static HtmlColor SlateBlue => new HtmlColor(ColorNameMap["slateblue"]);
        public static HtmlColor SlateGray => new HtmlColor(ColorNameMap["slategray"]);
        public static HtmlColor SlateGrey => new HtmlColor(ColorNameMap["slategrey"]);
        public static HtmlColor Snow => new HtmlColor(ColorNameMap["snow"]);
        public static HtmlColor SpringGreen => new HtmlColor(ColorNameMap["springgreen"]);
        public static HtmlColor SteelBlue => new HtmlColor(ColorNameMap["steelblue"]);
        public static HtmlColor Tan => new HtmlColor(ColorNameMap["tan"]);
        public static HtmlColor Teal => new HtmlColor(ColorNameMap["teal"]);
        public static HtmlColor Thistle => new HtmlColor(ColorNameMap["thistle"]);
        public static HtmlColor Tomato => new HtmlColor(ColorNameMap["tomato"]);
        public static HtmlColor Turquoise => new HtmlColor(ColorNameMap["turquoise"]);
        public static HtmlColor Violet => new HtmlColor(ColorNameMap["violet"]);
        public static HtmlColor Wheat => new HtmlColor(ColorNameMap["wheat"]);
        public static HtmlColor White => new HtmlColor(ColorNameMap["white"]);
        public static HtmlColor WhiteSmoke => new HtmlColor(ColorNameMap["whitesmoke"]);
        public static HtmlColor Yellow => new HtmlColor(ColorNameMap["yellow"]);
        public static HtmlColor YellowGreen => new HtmlColor(ColorNameMap["yellowgreen"]);
        public static HtmlColor Transparent => new HtmlColor(ColorNameMap["transparent"]);


        public int A { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public HtmlColor(int a, int r, int g, int b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
        private HtmlColor((int a, int r, int g, int b) argb)
        {
            A = argb.a;
            R = argb.r;
            G = argb.g;
            B = argb.b;
        }

        private static readonly Dictionary<string, (int, int, int, int)> ColorNameMap = new Dictionary<string, (int, int, int, int)>
        {
            { "aliceblue", (255, 240, 248, 255) },
            { "antiquewhite", (255, 250, 235, 215) },
            { "aqua", (255, 0, 255, 255) },
            { "aquamarine", (255, 127, 255, 212) },
            { "azure", (255, 240, 255, 255) },
            { "beige", (255, 245, 245, 220) },
            { "bisque", (255, 255, 228, 196) },
            { "black", (255, 0, 0, 0) },
            { "blanchedalmond", (255, 255, 235, 205) },
            { "blue", (255, 0, 0, 255) },
            { "blueviolet", (255, 138, 43, 226) },
            { "brown", (255, 165, 42, 42) },
            { "burlywood", (255, 222, 184, 135) },
            { "cadetblue", (255, 95, 158, 160) },
            { "chartreuse", (255, 127, 255, 0) },
            { "chocolate", (255, 210, 105, 30) },
            { "coral", (255, 255, 127, 80) },
            { "cornflowerblue", (255, 100, 149, 237) },
            { "cornsilk", (255, 255, 248, 220) },
            { "crimson", (255, 220, 20, 60) },
            { "cyan", (255, 0, 255, 255) },
            { "darkblue", (255, 0, 0, 139) },
            { "darkcyan", (255, 0, 139, 139) },
            { "darkgoldenrod", (255, 184, 134, 11) },
            { "darkgray", (255, 169, 169, 169) },
            { "darkgreen", (255, 0, 100, 0) },
            { "darkgrey", (255, 169, 169, 169) },
            { "darkkhaki", (255, 189, 183, 107) },
            { "darkmagenta", (255, 139, 0, 139) },
            { "darkolivegreen", (255, 85, 107, 47) },
            { "darkorange", (255, 255, 140, 0) },
            { "darkorchid", (255, 153, 50, 204) },
            { "darkred", (255, 139, 0, 0) },
            { "darksalmon", (255, 233, 150, 122) },
            { "darkseagreen", (255, 143, 188, 143) },
            { "darkslateblue", (255, 72, 61, 139) },
            { "darkslategray", (255, 47, 79, 79) },
            { "darkslategrey", (255, 47, 79, 79) },
            { "darkturquoise", (255, 0, 206, 209) },
            { "darkviolet", (255, 148, 0, 211) },
            { "deeppink", (255, 255, 20, 147) },
            { "deepskyblue", (255, 0, 191, 255) },
            { "dimgray", (255, 105, 105, 105) },
            { "dimgrey", (255, 105, 105, 105) },
            { "dodgerblue", (255, 30, 144, 255) },
            { "firebrick", (255, 178, 34, 34) },
            { "floralwhite", (255, 255, 250, 240) },
            { "forestgreen", (255, 34, 139, 34) },
            { "fuchsia", (255, 255, 0, 255) },
            { "gainsboro", (255, 220, 220, 220) },
            { "ghostwhite", (255, 248, 248, 255) },
            { "gold", (255, 255, 215, 0) },
            { "goldenrod", (255, 218, 165, 32) },
            { "gray", (255, 128, 128, 128) },
            { "green", (255, 0, 128, 0) },
            { "greenyellow", (255, 173, 255, 47) },
            { "grey", (255, 128, 128, 128) },
            { "honeydew", (255, 240, 255, 240) },
            { "hotpink", (255, 255, 105, 180) },
            { "indianred", (255, 205, 92, 92) },
            { "indigo", (255, 75, 0, 130) },
            { "ivory", (255, 255, 255, 240) },
            { "khaki", (255, 240, 230, 140) },
            { "lavender", (255, 230, 230, 250) },
            { "lavenderblush", (255, 255, 240, 245) },
            { "lawngreen", (255, 124, 252, 0) },
            { "lemonchiffon", (255, 255, 250, 205) },
            { "lightblue", (255, 173, 216, 230) },
            { "lightcoral", (255, 240, 128, 128) },
            { "lightcyan", (255, 224, 255, 255) },
            { "lightgoldenrodyellow", (255, 250, 250, 210) },
            { "lightgray", (255, 211, 211, 211) },
            { "lightgreen", (255, 144, 238, 144) },
            { "lightgrey", (255, 211, 211, 211) },
            { "lightpink", (255, 255, 182, 193) },
            { "lightsalmon", (255, 255, 160, 122) },
            { "lightseagreen", (255, 32, 178, 170) },
            { "lightskyblue", (255, 135, 206, 250) },
            { "lightslategray", (255, 119, 136, 153) },
            { "lightslategrey", (255, 119, 136, 153) },
            { "lightsteelblue", (255, 176, 224, 230) },
            { "lightyellow", (255, 255, 255, 224) },
            { "lime", (255, 0, 255, 0) },
            { "limegreen", (255, 50, 205, 50) },
            { "linen", (255, 250, 240, 230) },
            { "magenta", (255, 255, 0, 255) },
            { "maroon", (255, 128, 0, 0) },
            { "mediumaquamarine", (255, 102, 205, 170) },
            { "mediumblue", (255, 0, 0, 205) },
            { "mediumorchid", (255, 186, 85, 211) },
            { "mediumpurple", (255, 147, 112, 219) },
            { "mediumseagreen", (255, 60, 179, 113) },
            { "mediumslateblue", (255, 123, 104, 238) },
            { "mediumspringgreen", (255, 0, 250, 154) },
            { "mediumturquoise", (255, 72, 209, 204) },
            { "mediumvioletred", (255, 199, 21, 133) },
            { "midnightblue", (255, 25, 25, 112) },
            { "mintcream", (255, 245, 255, 250) },
            { "mistyrose", (255, 255, 228, 225) },
            { "moccasin", (255, 255, 228, 181) },
            { "navajowhite", (255, 255, 222, 173) },
            { "navy", (255, 0, 0, 128) },
            { "oldlace", (255, 253, 245, 230) },
            { "olive", (255, 128, 128, 0) },
            { "olivedrab", (255, 107, 142, 35) },
            { "orange", (255, 255, 165, 0) },
            { "orangered", (255, 255, 69, 0) },
            { "orchid", (255, 218, 112, 214) },
            { "palegoldenrod", (255, 238, 232, 170) },
            { "palegreen", (255, 152, 251, 152) },
            { "paleturquoise", (255, 175, 238, 238) },
            { "palevioletred", (255, 219, 112, 147) },
            { "papayawhip", (255, 255, 239, 213) },
            { "peachpuff", (255, 255, 218, 185) },
            { "peru", (255, 205, 133, 63) },
            { "pink", (255, 255, 192, 203) },
            { "plum", (255, 221, 160, 221) },
            { "powderblue", (255, 176, 224, 230) },
            { "purple", (255, 128, 0, 128) },
            { "rebeccapurple", (255, 102, 51, 153) },
            { "red", (255, 255, 0, 0) },
            { "rosybrown", (255, 188, 143, 143) },
            { "royalblue", (255, 65, 105, 225) },
            { "saddlebrown", (255, 139, 69, 19) },
            { "salmon", (255, 250, 128, 114) },
            { "sandybrown", (255, 244, 164, 96) },
            { "seagreen", (255, 46, 139, 87) },
            { "seashell", (255, 255, 245, 238) },
            { "sienna", (255, 160, 82, 45) },
            { "silver", (255, 192, 192, 192) },
            { "skyblue", (255, 135, 206, 235) },
            { "slateblue", (255, 106, 90, 205) },
            { "slategray", (255, 112, 128, 144) },
            { "slategrey", (255, 112, 128, 144) },
            { "snow", (255, 255, 250, 250) },
            { "springgreen", (255, 0, 255, 127) },
            { "steelblue", (255, 70, 130, 180) },
            { "tan", (255, 210, 180, 140) },
            { "teal", (255, 0, 128, 128) },
            { "thistle", (255, 216, 191, 216) },
            { "tomato", (255, 255, 99, 71) },
            { "turquoise", (255, 64, 224, 208) },
            { "violet", (255, 238, 130, 238) },
            { "wheat", (255, 245, 222, 179) },
            { "white", (255, 255, 255, 255) },
            { "whitesmoke", (255, 245, 245, 245) },
            { "yellow", (255, 255, 255, 0) },
            { "yellowgreen", (255, 154, 205, 50) },
            { "transparent", (0, 255, 255, 255) }
        };

        public static HtmlColor Parse(string color)
        {
            if (color.StartsWith("#"))
            {
                if (color.Length == 7) // #RRGGBB
                {
                    return new HtmlColor(255,
                        Convert.ToInt32(color.Substring(1, 2), 16),
                        Convert.ToInt32(color.Substring(3, 2), 16),
                        Convert.ToInt32(color.Substring(5, 2), 16));
                }
                else if (color.Length == 9) // #AARRGGBB
                {
                    return new HtmlColor(
                        Convert.ToInt32(color.Substring(1, 2), 16),
                        Convert.ToInt32(color.Substring(3, 2), 16),
                        Convert.ToInt32(color.Substring(5, 2), 16),
                        Convert.ToInt32(color.Substring(7, 2), 16));
                }
            }
            else if (ColorNameMap.TryGetValue(color.ToLower(), out var rgba))
            {
                return new HtmlColor(rgba.Item1, rgba.Item2, rgba.Item3, rgba.Item4);
            }

            throw new FormatException("Invalid color format.");
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool useColorName)
        {
            if (useColorName)
            {
                var colorName = ColorNameMap.FirstOrDefault(x => x.Value == (A, R, G, B)).Key;
                if (colorName != null)
                {
                    return colorName;
                }
            }

            if (A == 255)
            {
                return $"#{R:X2}{G:X2}{B:X2}";
            }
            else
            {
                return $"#{A:X2}{R:X2}{G:X2}{B:X2}";
            }
        }
    }
}
