using NUnit.Framework;
using UI;
using UnityEngine;

public class BalloonPositionTests
{
    // EXPECTED VALUES
    // a 0      x +4  z 0
    // a 90     x 0   z -4
    // a 180    x -4  z 0
    // a 270    x 0   z +4  
    
    [Test]
    public void CheckPositionAngle0()
    {
        const float angleHumanToCamera = 0f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, bb.distance);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, 0);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle045()
    {
        const float angleHumanToCamera = 45f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, bb.distance/2);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, -bb.distance/2);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle090()
    {
        const float angleHumanToCamera = 90f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, 0);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, -bb.distance);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle135()
    {
        const float angleHumanToCamera = 135f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, -bb.distance/2);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, -bb.distance/2);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle180()
    {
        const float angleHumanToCamera = 180f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, -bb.distance);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, 0);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle225()
    {
        const float angleHumanToCamera = 225f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, -bb.distance/2);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, bb.distance/2);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle270()
    {
        const float angleHumanToCamera = 270f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, 0);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, bb.distance);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
    
    [Test]
    public void CheckPositionAngle315()
    {
        const float angleHumanToCamera = 315f;
        
        var gameObj = new GameObject();
        var bb = gameObj.AddComponent<BalloonBillboard>();
        
        var xDestination = bb.GetPositionX(angleHumanToCamera);
        var isXCloseEnough = Mathf.Approximately(xDestination, bb.distance/2);
        Assert.IsTrue(isXCloseEnough, "x value is incorrect");
        
        var zDestination = bb.GetPositionZ(angleHumanToCamera);
        var isZCloseEnough = Mathf.Approximately(zDestination, bb.distance/2);
        Assert.IsTrue(isZCloseEnough, "z value is incorrect");
    }
}
