using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers
{
    internal class TextRenderer : TextObject
    {
        private readonly List<Rectangle> _bounds = new List<Rectangle>();

        public TextRenderer()
        {
            FirstNode = this;
        }

        protected TextObject FirstNode;
        protected TextObject DrawPointer;

        public override void Destroy()
        {
            //Clear();
        }

        public virtual void Update(double totalTime, double frameTime)
        {
            ProcessWorldText(false);
        }

        public void Select(int startX, int startY, int renderIndex, bool isGump = false)
        {
            int mouseX = Mouse.Position.X;
            int mouseY = Mouse.Position.Y;

            for (TextObject item = DrawPointer; item != null; item = item.DLeft)
            {
                if (item.RenderedText == null || item.RenderedText.IsDestroyed || item.RenderedText.Texture == null)
                {
                    continue;
                }

                if (item.Time >= ClassicUO.Time.Ticks)
                {
                    if (item.Owner == null || item.Owner.UseInRender != renderIndex)
                    {
                        continue;
                    }
                }

                if (item.RenderedText.Texture.Contains
                    (mouseX - startX - item.RealScreenPosition.X, mouseY - startY - item.RealScreenPosition.Y))
                {
                    SelectedObject.LastObject = item;
                }
            }

            if (SelectedObject.LastObject is TextObject t)
            {
                if (isGump)
                {
                    if (t.IsTextGump)
                    {
                        t.ToTopD();
                    }
                }
                else
                {
                    MoveToTop(t);
                }
            }
        }

        public virtual void Draw(UltimaBatcher2D batcher, int startX, int startY, int renderIndex, bool isGump = false)
        {
            ProcessWorldText(false);

            int mouseX = Mouse.Position.X;
            int mouseY = Mouse.Position.Y;

            BaseGameObject last = SelectedObject.LastObject;

            for (TextObject o = DrawPointer; o != null; o = o.DLeft)
            {
                if (o.IsDestroyed || o.RenderedText == null || o.RenderedText.IsDestroyed ||
                    o.RenderedText.Texture == null || o.Time < ClassicUO.Time.Ticks ||
                    o.Owner.UseInRender != renderIndex && !isGump)
                {
                    continue;
                }

                ushort hue = 0;

                float alpha = 1f - o.Alpha / 255f;

                if (o.IsTransparent)
                {
                    if (o.Alpha == 0xFF)
                    {
                        alpha = 1f - 0x7F / 255f;
                    }
                }

                int x = o.RealScreenPosition.X;
                int y = o.RealScreenPosition.Y;

                if (o.RenderedText.Texture.Contains(mouseX - x - startX, mouseY - y - startY))
                {
                    if (isGump)
                    {
                        SelectedObject.LastObject = o;
                    }
                    else
                    {
                        SelectedObject.Object = o;
                    }
                }

                if (!isGump)
                {
                    if (o.Owner is Entity && last == o)
                    {
                        hue = 0x0035;
                    }
                }
                else
                {
                    x += startX;
                    y += startY;
                }

                o.RenderedText.Draw(batcher, x, y, alpha, hue);
            }
        }

        public void MoveToTop(TextObject obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.UnlinkD();

            TextObject next = FirstNode.DRight;
            FirstNode.DRight = obj;
            obj.DLeft = FirstNode;
            obj.DRight = next;

            if (next != null)
            {
                next.DLeft = obj;
            }
        }

        public void ProcessWorldText(bool doit)
        {
            if (doit)
            {
                if (_bounds.Count != 0)
                {
                    _bounds.Clear();
                }
            }

            for (DrawPointer = FirstNode; DrawPointer != null; DrawPointer = DrawPointer.DRight)
            {
                if (doit)
                {
                    TextObject t = DrawPointer;

                    if (t.Time >= ClassicUO.Time.Ticks && t.RenderedText != null && !t.RenderedText.IsDestroyed)
                    {
                        if (t.Owner != null)
                        {
                            t.IsTransparent = Collides(t);
                            CalculateAlpha(t);
                        }
                    }
                }

                if (DrawPointer.DRight == null)
                {
                    break;
                }
            }
        }

        private void CalculateAlpha(TextObject msg)
        {
            if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.TextFading)
            {
                int delta = (int) (msg.Time - ClassicUO.Time.Ticks);

                if (delta >= 0 && delta <= 1000)
                {
                    delta /= 10;

                    if (delta > 100)
                    {
                        delta = 100;
                    }
                    else if (delta < 1)
                    {
                        delta = 0;
                    }

                    delta = 255 * delta / 100;

                    if (!msg.IsTransparent || delta <= 0x7F)
                    {
                        msg.Alpha = (byte) delta;
                    }

                    msg.IsTransparent = true;
                }
            }
        }

        private bool Collides(TextObject msg)
        {
            bool result = false;

            Rectangle rect = new Rectangle
            {
                X = msg.RealScreenPosition.X,
                Y = msg.RealScreenPosition.Y,
                Width = msg.RenderedText.Width,
                Height = msg.RenderedText.Height
            };

            for (int i = 0; i < _bounds.Count; i++)
            {
                if (_bounds[i].Intersects(rect))
                {
                    result = true;

                    break;
                }
            }

            _bounds.Add(rect);

            return result;
        }

        public void AddMessage(TextObject obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.UnlinkD();

            TextObject item = FirstNode;

            if (item != null)
            {
                if (item.DRight != null)
                {
                    TextObject next = item.DRight;

                    item.DRight = obj;
                    obj.DLeft = item;
                    obj.DRight = next;
                    next.DLeft = obj;
                }
                else
                {
                    item.DRight = obj;
                    obj.DLeft = item;
                    obj.DRight = null;
                }
            }
        }


        public new virtual void Clear()
        {
            if (FirstNode != null)
            {
                TextObject first = FirstNode;

                while (first?.DLeft != null)
                {
                    first = first.DLeft;
                }

                while (first != null)
                {
                    TextObject next = first.DRight;

                    first.Destroy();
                    first.Clear();

                    first = next;
                }
            }

            if (DrawPointer != null)
            {
                TextObject first = DrawPointer;

                while (first?.DLeft != null)
                {
                    first = first.DLeft;
                }

                while (first != null)
                {
                    TextObject next = first.DRight;

                    first.Destroy();
                    first.Clear();

                    first = next;
                }
            }

            FirstNode = this;
            FirstNode.DLeft = null;
            FirstNode.DRight = null;
            DrawPointer = null;
        }
    }
}