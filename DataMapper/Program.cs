namespace DataMapper
{
    using System;

    struct Vector4Struct
    {
        public float x;
        public float y;
        private float z;
        private float w;

        public Vector4Struct(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        #region Overrides of ValueType

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        #endregion
    }

    class Vector4Class
    {
        private float x;
        private float y;
        public float z;
        public float w;

        public Vector4Class(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        #region Overrides of ValueType

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        #endregion
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var mapper = new IntersectMapper();
            var c = new Vector4Class(1, 2, 3, 4);
            var cc = mapper.Map<Vector4Class, Vector4Struct>(c);
            Console.WriteLine("source: {0}, dest: {1}", c, cc);
            Console.ReadKey();
        }
    }
}
