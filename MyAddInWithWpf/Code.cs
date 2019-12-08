// Source: https://adndevblog.typepad.com/manufacturing/2013/01/inventor-eulerian-angles-of-assembly-component.html

using Inventor;
using System;

class TransformConverter
{
    public static URDF.Origin CalculateRotationAngles(Matrix oMatrix, Application _invApp)
    {
        const double PI = 3.14159265358979;
        double dB;
        double dC;
        double dNumer;
        double dDenom;
        double dAcosValue;

        double[] aRotAngles = new double[3];

        Matrix oRotate = _invApp.TransientGeometry.CreateMatrix();
        Vector oAxis = _invApp.TransientGeometry.CreateVector();
        Point oCenter = _invApp.TransientGeometry.CreatePoint();
        
        oCenter.X = 0;
        oCenter.Y = 0;
        oCenter.Z = 0;

        // Choose aRotAngles[0] about x which transforms axes[2] onto the x-z plane
        dB = oMatrix.Cell[2,3];
        dC = oMatrix.Cell[3,3];

        dNumer = dC;

        dDenom = Math.Sqrt(dB * dB + dC * dC);


        // Make sure we can do the division.  If not, then axes[2] is already in the x-z plane
        if ((Math.Abs(dDenom) <= 0.000001))
        {
            aRotAngles[0] = 0.0;
        }
        else
        {
            if ((dNumer / dDenom >= 1.0))
                dAcosValue = 0.0;
            else if ((dNumer / dDenom <= -1.0))
                dAcosValue = PI;
            else
                dAcosValue = Math.Acos(dNumer / dDenom);

            aRotAngles[0] = Math.Sign(dB) * dAcosValue;
            oAxis.X = 1;
            oAxis.Y = 0;
            oAxis.Z = 0;

            oRotate.SetToRotation(aRotAngles[0], oAxis, oCenter);
            oMatrix.PreMultiplyBy(oRotate);
        }

        // 
        // Choose aRotAngles[1] about y which transforms axes[3] onto the z axis
        // 
        if ((oMatrix.Cell[3, 3] >= 1.0))
            dAcosValue = 0.0;
        else if ((oMatrix.Cell[3, 3] <= -1.0))
            dAcosValue = PI;
        else
            dAcosValue = Math.Acos(oMatrix.Cell[3, 3]);

        aRotAngles[1] = Math.Sign(-oMatrix.Cell[1, 3]) * dAcosValue;

        oAxis.X = 0;
        oAxis.Y = 1;
        oAxis.Z = 0;

        oRotate.SetToRotation(aRotAngles[1], oAxis, oCenter);
        oMatrix.PreMultiplyBy(oRotate);

        // 
        // Choose aRotAngles[2] about z which transforms axes[0] onto the x axis
        // 
        if ((oMatrix.Cell[1, 1] >= 1.0))
            dAcosValue = 0.0;
        else if ((oMatrix.Cell[1, 1] <= -1.0))
            dAcosValue = PI;
        else
            dAcosValue = Math.Acos(oMatrix.Cell[1, 1]);

        aRotAngles[2] = Math.Sign(-oMatrix.Cell[2, 1]) * dAcosValue;

        URDF.Origin output = new URDF.Origin();

        output.RPY = aRotAngles;
        //output.XYZ[0] = oMatrix.Cell[1, 4];
        //output.XYZ[1] = oMatrix.Cell[2, 4];
        //output.XYZ[2] = oMatrix.Cell[3, 4];

        return output;
    }
}