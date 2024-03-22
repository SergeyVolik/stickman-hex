using NUnit.Framework;
using Prototype;
using System;
using UnityEngine;

public class Prototype_Tests
{
    ResourceTypeSO m_WoodType;
    ResourceTypeSO m_StoneType;

    [SetUp]
    public void Setup()
    {
        m_WoodType = ScriptableObject.CreateInstance<ResourceTypeSO>();
        m_StoneType = ScriptableObject.CreateInstance<ResourceTypeSO>();
    }

    [TearDown]
    public void TearDown()
    {
        ScriptableObject.Destroy(m_WoodType);
        ScriptableObject.Destroy(m_StoneType);
    }

    [Test]
    public void Test_ResourceContainer_AddResource()
    {
        var resources = new ResourceContainer();

        Assert.IsTrue(resources.GetResource(m_WoodType) == 0);

        resources.AddResource(m_WoodType, 10);

        Assert.IsTrue(resources.GetResource(m_WoodType) == 10);

        resources.AddResource(m_StoneType, 5);

        Assert.IsTrue(resources.GetResource(m_StoneType) == 5);
    }

    [Test]
    public void Test_ResourceContainer_ResourceChangedEvent() 
    {
        var resources = new ResourceContainer();

        TestEventResourceEvent(resources, m_WoodType, 10);
        TestEventResourceEvent(resources, m_WoodType, 5);
        TestEventResourceEvent(resources, m_WoodType, 0);
        TestEventResourceEvent(resources, m_StoneType, 0);
        TestEventResourceEvent(resources, m_StoneType, 10);
        TestEventResourceEvent(resources, m_StoneType, 5);
    }

    private void TestEventResourceEvent(ResourceContainer resources, ResourceTypeSO type, int count)
    {
        Action<ResourceTypeSO, int> resAction = (res, newValue) =>
        {
            Assert.IsTrue(res == type && newValue == count);
        };

        resources.onResourceChanged += resAction;
        resources.SetResource(type, count);
        resources.onResourceChanged -= resAction;
    }

    [Test]
    public void Test_ResourceContainer_SetResource()
    {
        var resources = new ResourceContainer();

        Assert.IsTrue(resources.GetResource(m_WoodType) == 0);

        resources.SetResource(m_WoodType, 10);
        Assert.IsTrue(resources.GetResource(m_WoodType) == 10);
        resources.SetResource(m_WoodType, 5);
        Assert.IsTrue(resources.GetResource(m_WoodType) == 5);

        resources.SetResource(m_StoneType, 5);
        Assert.IsTrue(resources.GetResource(m_StoneType) == 5);

        resources.SetResource(m_StoneType, 0);
        Assert.IsTrue(resources.GetResource(m_StoneType) == 0);
    }

    [Test]
    public void Test_ResourceContainer_RemoveResource()
    {
        var resources = new ResourceContainer();

        Assert.IsTrue(resources.GetResource(m_WoodType) == 0);

        resources.AddResource(m_WoodType, 10);

        Assert.IsTrue(resources.GetResource(m_WoodType) == 10);

        resources.RemoveResource(m_WoodType, 10);

        Assert.IsTrue(resources.GetResource(m_WoodType) == 0);
    }

    [Test]
    public void Test_ResourceContainer_IsEmpty()
    {
        var resources = new ResourceContainer();

        Assert.IsTrue(resources.IsEmpty() == true);

        resources.AddResource(m_WoodType, 10);
        resources.AddResource(m_StoneType, 5);

        Assert.IsTrue(resources.IsEmpty() == false);

        resources.RemoveResource(m_WoodType, 10);
        resources.RemoveResource(m_StoneType, 5);

        Assert.IsTrue(resources.IsEmpty() == true);
    }
}
