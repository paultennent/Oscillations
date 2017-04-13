using System;

namespace stdInput
{
#pragma warning disable 649
    internal class Command
    {
        public String type;
        public int keyCode;
        public String character;
        public String action;
        public Coordinates coords;
        public float value;
    }
#pragma warning restore 649

    public class Coordinates
    {
        public int x;
        public int y;
        public int z;
    }
}