﻿using ColossalFramework.Plugins;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// High level PO wrapper, always available

namespace MoveIt
{
    internal class PO_Manager
    {
        private IPO_Logic Lower;

        private HashSet<uint> visibleIds = new HashSet<uint>();
        private HashSet<uint> selectedIds = new HashSet<uint>();
        private Dictionary<uint, IPO_Object> visibleObjects = new Dictionary<uint, IPO_Object>();

        internal List<IPO_Object> Objects => new List<IPO_Object>(visibleObjects.Values);
        internal IPO_Object GetProcObj(uint id) => visibleObjects[id];

        internal PO_Manager()
        {
            try
            {
                InitialiseLogic();
            }
            catch (TypeLoadException ex)
            {
                Lower = new PO_LogicDisabled();
            }
        }

        private void InitialiseLogic()
        {
            if (isPOEnabled())
            {
                Lower = new PO_LogicEnabled();
            }
            else
            {
                Lower = new PO_LogicDisabled();
            }
        }

        internal bool ToolEnabled()
        {
            Dictionary<uint, IPO_Object> newVisible = new Dictionary<uint, IPO_Object>();
            HashSet<uint> newIds = new HashSet<uint>();

            foreach (IPO_Object obj in Lower.Objects)
            {
                newVisible.Add(obj.Id, obj);
                newIds.Add(obj.Id);
            }

            HashSet<uint> removed = new HashSet<uint>(visibleIds);
            removed.ExceptWith(newIds);
            HashSet<uint> added = new HashSet<uint>(newIds);
            added.ExceptWith(visibleIds);
            HashSet<uint> newSelectedIds = new HashSet<uint>(selectedIds);
            newSelectedIds.IntersectWith(newIds);

            List<Instance> toRemove = new List<Instance>();
            foreach (Instance instance in Action.selection)
            {
                Debug.Log(instance);
                uint id = instance.id.NetLane;
                if (id > 0)
                {
                    if (removed.Contains(id))
                    {
                        toRemove.Add(instance);
                    }
                }
            }
            foreach (Instance instance in toRemove)
            {
                Action.selection.Remove(instance);
            }

            Debug.Log($"Visible from:{visibleObjects.Count} to:{newVisible.Count}\n" +
                $"Selected from:{selectedIds.Count} to:{newSelectedIds.Count}");

            visibleObjects = newVisible;
            visibleIds = newIds;
            selectedIds = newSelectedIds;

            // Has anything changed?
            if (added.Count > 0 || removed.Count > 0)
                return true;

            return false;
        }

        internal void SelectionAdd(HashSet<Instance> instances)
        {
            foreach (Instance i in instances)
            {
                SelectionAdd(i);
            }
        }

        internal void SelectionAdd(Instance instance)
        {
            if (instance.id.NetLane <= 0) return;

            selectedIds.Add(instance.id.NetLane);
        }

        internal void SelectionRemove(HashSet<Instance> instances)
        {
            foreach (Instance i in instances)
            {
                SelectionRemove(i);
            }
        }

        internal void SelectionRemove(Instance instance)
        {
            if (instance.id.NetLane <= 0) return;

            selectedIds.Remove(instance.id.NetLane);
        }

        internal void SelectionClear()
        {
            selectedIds.Clear();
        }


        internal static bool isPOEnabled()
        {
            Debug.Log(PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == 1094334744uL || mod.name.Contains("ProceduralObjects") || mod.name.Contains("Procedural Objects")) && mod.isEnabled).ToString());

            string msg = "\n";
            foreach (PluginManager.PluginInfo pi in PluginManager.instance.GetPluginsInfo())
            {
                msg += $"{pi.name} #{pi.publishedFileID}\n";
            }
            ModInfo.DebugLine(msg);

            ModInfo.DebugLine(PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == 1094334744uL || mod.name.Contains("ProceduralObjects") || mod.name.Contains("Procedural Objects")) && mod.isEnabled).ToString());

            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == 1094334744uL || mod.name.Contains("ProceduralObjects") || mod.name.Contains("Procedural Objects")) && mod.isEnabled);
        }
    }


    // PO Logic
    internal interface IPO_Logic
    {
        List<IPO_Object> Objects { get; }
    }

    internal class PO_LogicDisabled : IPO_Logic
    {
        public List<IPO_Object> Objects
        {
            get
            {
                Debug.Log($"PO List: Inactive");
                return new List<IPO_Object>();
            }
        }
    }


    // PO Object
    internal interface IPO_Object
    {
        uint Id { get; set; } // The InstanceID.NetLane value
        Vector3 Position { get; set; }
        float Angle { get; set; }
        void SetPositionY(float h);
        float GetDistance(Vector3 location);
        void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color color);
        string DebugQuaternion();
    }

    internal class PO_ObjectInactive : IPO_Object
    {
        public uint Id { get; set; } // The InstanceID.NetLane value

        public Vector3 Position
        {
            get => Vector3.zero;
            set { }
        }

        public float Angle
        {
            get => 0f;
            set { }
        }

        public void SetPositionY(float h)
        {
            return;
        }

        public float GetDistance(Vector3 location) => 0f;

        public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color color)
        { }

        public string DebugQuaternion()
        {
            return "";
        }
    }
}
