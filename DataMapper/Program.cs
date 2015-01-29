namespace DataMapper
{
    using System;

    struct Vector4Struct
    {
        public float x;
        public float y;
        private float z;
        private float w;

        public float CachedMagnitude { get; private set; }

        public Vector4Struct(float x, float y, float z, float w) : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;

            CachedMagnitude = (float) Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        #region Overrides of ValueType

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3}) = {4}", x, y, z, w, CachedMagnitude);
        }

        #endregion
    }

    class Vector4Class
    {
        private float x;
        private float y;
        public float z;
        public float w;

        public float CachedMagnitude { get; private set; }

        public Vector4Class(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            CachedMagnitude = 42;
        }

        #region Overrides of ValueType

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3}) = {4}", x, y, z, w, CachedMagnitude);
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

            /*
            var s = new Vector4Struct(1, 4, 9, 16);
            var sc = mapper.Map<Vector4Struct, Vector4Class>(s);
            Console.WriteLine("source: {0}, dest: {1}", s, sc);
             */

            Console.WriteLine("Press any key to close the window...");
            Console.ReadKey();
        }
    }
}
