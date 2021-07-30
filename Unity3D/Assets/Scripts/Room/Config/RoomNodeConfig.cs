using System.Collections.Generic;



public class RoomNodeConfig
{
    public class Item
    {
        public string id;
        public bool isOld;
        public Dictionary<string, Offset> offsets = new Dictionary<string, Offset>(4);
        

        public class Offset
        {
            public int offsetX;
            public int offsetY;
        }
    }
    
    
    public long id;
    public List<Item> items = new List<Item>();
    public int renderQueue;
    
    public string beizhu;
}


