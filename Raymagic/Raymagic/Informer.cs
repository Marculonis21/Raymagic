using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Informer
    {
        Graphics graphics;
        Dictionary<string, string> infoList = new Dictionary<string, string>();
        const int heightOffset = 15;

        //SINGLETON
        private Informer()
        {}

        public static readonly Informer instance = new Informer();

        public void SetGraphics(Graphics graphics)
        {
            this.graphics = graphics;
        }

        public void AddInfo(string key, string info)
        {
            if(infoList.ContainsKey(key))
                infoList[key] = info;
            else
                infoList.Add(key,info);

        }

        public void ShowInfo(Vector2 origin, SpriteFont font, Color color)
        {
            int count = 0;
            foreach(string s in infoList.Values)
            {
                graphics.DrawText(s, font, origin + new Vector2(0,1)*heightOffset*count, color, 0,0);
                count++;
            }
            infoList.Clear();
        }
    }
}
