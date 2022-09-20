using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Informer
    {
        Graphics graphics;
        Dictionary<string, string> infoList = new Dictionary<string, string>();
        Dictionary<string, string> infoListPersistent = new Dictionary<string, string>();
        const int heightOffset = 15;

        //SINGLETON
        private Informer()
        {}

        public static readonly Informer instance = new Informer();

        public void SetGraphics(Graphics graphics)
        {
            this.graphics = graphics;
        }

        public void AddInfo(string key, string info, bool persistent=false)
        {
            if (persistent)
            {
                if(infoListPersistent.ContainsKey(key))
                    infoListPersistent[key] = info;
                else
                    infoListPersistent.Add(key,info);
            }

            else
            {
                if(infoList.ContainsKey(key))
                    infoList[key] = info;
                else
                    infoList.Add(key,info);
            }
        }
        public void RemoveInfo(string key)
        {
            if (infoListPersistent.ContainsKey(key))
            {
                infoListPersistent.Remove(key);
            }

            if (infoList.ContainsKey(key))
            {
                infoList.Remove(key);
            }
        }

        public void ShowInfo(Vector2 origin, SpriteFont font, Color color)
        {
            int count = 0;
            foreach(string s in infoList.Values)
            {
                graphics.DrawText(s, font, origin + new Vector2(0,1)*heightOffset*count, color, 0,0);
                count++;
            }
            count += 2;

            foreach(string s in infoListPersistent.Values)
            {
                graphics.DrawText(s, font, origin + new Vector2(0,1)*heightOffset*count, color, 0,0);
                count++;
            }
            infoList.Clear();
        }
    }
}
