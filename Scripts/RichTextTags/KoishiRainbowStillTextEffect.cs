using Godot;
using MegaCrit.Sts2.Core.RichTextTags;

namespace KomeijiKoishi.RichTextTags
{
    public sealed partial class KoishiRainbowStillTextEffect : AbstractMegaRichTextEffect
    {
        protected override string Bbcode => "koishi_rainbow_still";

        public override bool _ProcessCustomFX(CharFXTransform charFx)
        {
            if (!ShouldTransformText())
            {
                return false;
            }

            float hue = (float)(charFx.ElapsedTime * 0.32 + charFx.RelativeIndex * 0.075);
            hue -= Mathf.Floor(hue);
            charFx.Color = FromHsv(hue, 0.72f, 1.0f, charFx.Color.A);
            return true;
        }

        private static Color FromHsv(float hue, float saturation, float value, float alpha)
        {
            float sector = hue * 6f;
            float chroma = value * saturation;
            float x = chroma * (1f - Mathf.Abs(sector % 2f - 1f));
            float m = value - chroma;

            float r;
            float g;
            float b;
            if (sector < 1f)
            {
                r = chroma;
                g = x;
                b = 0f;
            }
            else if (sector < 2f)
            {
                r = x;
                g = chroma;
                b = 0f;
            }
            else if (sector < 3f)
            {
                r = 0f;
                g = chroma;
                b = x;
            }
            else if (sector < 4f)
            {
                r = 0f;
                g = x;
                b = chroma;
            }
            else if (sector < 5f)
            {
                r = x;
                g = 0f;
                b = chroma;
            }
            else
            {
                r = chroma;
                g = 0f;
                b = x;
            }

            return new Color(r + m, g + m, b + m, alpha);
        }
    }
}
