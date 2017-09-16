using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;

namespace Trajectories
{
    /// <summary>
    /// Moveable objects need seperate handling for trajectory compression or index data structures degrading with dynamic data types
    /// </summary>
    public interface IMoveable
    {
        Vector3D getVelocity();
        Point3D getPosition();
        Vector3D getMovementVector();
    }
}
