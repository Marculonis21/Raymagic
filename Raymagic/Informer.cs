using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Informer
    {
        Shapes shapes;
        Dictionary<string, string> infoList = new Dictionary<string, string>();
        const int heightOffset = 15;

        //SINGLETON
        private Informer()
        {}

        public static readonly Informer instance = new Informer();

        public void SetShapes(Shapes shapes)
        {
            this.shapes = shapes;
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
                shapes.DrawText(s, font, origin + new Vector2(0,1)*heightOffset*count, color, 0,0);
                count++;
            }
            infoList.Clear();
        }
    }
}
