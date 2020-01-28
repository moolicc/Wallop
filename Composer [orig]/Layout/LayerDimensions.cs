namespace Wallop.Composer.Layout
{
    class LayerDimensions
    {
        /// <summary>
        /// Gets or sets a value indicating the index of the origin monitor.
        /// This index is pre-transormation, meaning 0 will always be the top-left-most monitor.
        /// </summary>
        public int ReferenceMonitor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not coordinates are in absolute
        /// pixels or percentages that are based on the total screen size.
        /// </summary>
        public bool AbsoluteValues { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether or not the Z/W values represent width/height
        /// or distances from the right/bottom respectively.
        /// </summary>
        public bool MarginValues { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public LayerDimensions()
            : this(0, 0, 0, 100, 100, true, false)
        {

        }

        public LayerDimensions(int referenceMonitor, float x, float y, float z, float w)
            : this(referenceMonitor, x, y, z, w, true, false)
        {

        }

        public LayerDimensions(int referenceMonitor, float x, float y, float z, float w, bool absoluteValues, bool marginValues)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            ReferenceMonitor = referenceMonitor;
            AbsoluteValues = absoluteValues;
            MarginValues = marginValues;
        }
    }
}
