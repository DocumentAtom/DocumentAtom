namespace DocumentAtom.Core.Atoms
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Bounding box.
    /// </summary>
    public class BoundingBox
    {
        #region Public-Members

        /// <summary>
        /// Upper left corner.
        /// </summary>
        public Point UpperLeft
        {
            get => _UpperLeft;
            set
            {
                if (value.X < 0 || value.Y < 0)
                    throw new ArgumentOutOfRangeException(nameof(UpperLeft), "Coordinates must be zero or greater");
                _UpperLeft = value;
            }
        }

        /// <summary>
        /// Lower left corner.
        /// </summary>
        public Point LowerLeft
        {
            get => _LowerLeft;
            set
            {
                if (value.X < 0 || value.Y < 0)
                    throw new ArgumentOutOfRangeException(nameof(LowerLeft), "Coordinates must be zero or greater");
                _LowerLeft = value;
            }
        }

        /// <summary>
        /// Upper right corner.
        /// </summary>
        public Point UpperRight
        {
            get => _UpperRight;
            set
            {
                if (value.X < 0 || value.Y < 0)
                    throw new ArgumentOutOfRangeException(nameof(UpperRight), "Coordinates must be zero or greater");
                _UpperRight = value;
            }
        }

        /// <summary>
        /// Lower right corner.
        /// </summary>
        public Point LowerRight
        {
            get => _LowerRight;
            set
            {
                if (value.X < 0 || value.Y < 0)
                    throw new ArgumentOutOfRangeException(nameof(LowerRight), "Coordinates must be zero or greater");
                _LowerRight = value;
            }
        }

        /// <summary>
        /// Width.
        /// </summary>
        public int Width => UpperRight.X - UpperLeft.X;

        /// <summary>
        /// Height.
        /// </summary>
        public int Height => UpperLeft.Y - LowerLeft.Y;

        #endregion

        #region Private-Members

        private Point _UpperLeft = new Point(0, 0);
        private Point _LowerLeft = new Point(0, 0);
        private Point _UpperRight = new Point(0, 0);
        private Point _LowerRight = new Point(0, 0);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Bounding box.
        /// </summary>
        public BoundingBox()
        {

        }

        /// <summary>
        /// Create a bounding box from a rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <returns>Bounding box.</returns>
        public static BoundingBox FromRectangle(Rectangle rect)
        {
            return new BoundingBox
            {
                UpperLeft = new Point(rect.Left, rect.Top),
                LowerLeft = new Point(rect.Left, rect.Bottom),
                UpperRight = new Point(rect.Right, rect.Top),
                LowerRight = new Point(rect.Right, rect.Bottom)
            };
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Check if a point is contained within the bounding box.
        /// </summary>
        /// <param name="pointX">Point X.</param>
        /// <param name="pointY">Point Y.</param>
        /// <returns>True if contained within the bounding box.</returns>
        public bool Contains(int pointX, int pointY)
        {
            return pointX >= UpperLeft.X &&
                   pointX <= UpperRight.X &&
                   pointY >= UpperLeft.Y &&
                   pointY <= LowerLeft.Y;
        }

        /// <summary>
        /// Check if a bounding box intersects with this bounding box.
        /// </summary>
        /// <param name="other">Another bounding box.</param>
        /// <returns>True if the other bounding box intersects with this bounding box.</returns>
        public bool Intersects(BoundingBox other)
        {
            return UpperLeft.X < other.UpperRight.X &&
                   UpperRight.X > other.UpperLeft.X &&
                   UpperLeft.Y < other.LowerLeft.Y &&
                   LowerLeft.Y > other.UpperLeft.Y;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}