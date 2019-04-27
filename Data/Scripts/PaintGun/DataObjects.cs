﻿using System;
using System.Collections.Generic;
using ProtoBuf;
using VRage.ModAPI;
using VRageMath;

namespace Digi.PaintGun
{
    public class PlayerColorData
    {
        public ulong SteamId;
        public List<Vector3> Colors;
        public int SelectedSlot = 0;

        public PlayerColorData(ulong steamId, List<Vector3> colors)
        {
            this.SteamId = steamId;
            this.Colors = colors;
        }
    }

    public enum PacketAction
    {
        PAINT_BLOCK = 0,
        CONSUME_AMMO,
        GUN_FIRING_ON,
        GUN_FIRING_OFF,
        COLOR_PICK_ON,
        COLOR_PICK_OFF,
        BLOCK_REPLACE_COLOR,
        SELECTED_COLOR_SLOT,
        SET_COLOR,
        UPDATE_COLOR,
        UPDATE_COLOR_LIST,
        REQUEST_COLOR_LIST,
    }

    [Flags]
    public enum OddAxis
    {
        NONE = 0,
        X = 1,
        Y = 2,
        Z = 4
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketData
    {
        [ProtoMember]
        public PacketAction Type = PacketAction.PAINT_BLOCK;

        [ProtoMember]
        public ulong SteamId = 0;

        [ProtoMember]
        public long EntityId = 0;

        [ProtoMember]
        public uint PackedColor = 0;

        [ProtoMember]
        public uint PackedColor2 = 0;

        [ProtoMember]
        public byte Slot = 0;

        [ProtoMember]
        public OddAxis OddAxis = OddAxis.NONE;

        [ProtoMember]
        public bool UseGridSystem = false;

        [ProtoMember]
        public Vector3I? GridPosition = null;

        [ProtoMember]
        public Vector3I? MirrorPlanes = null;

        [ProtoMember]
        public uint[] PackedColors = null;

        public PacketData() { } // empty ctor is required for deserialization

        public override string ToString()
        {
            return $"Type={Type}\n" +
                $"SteamId={SteamId}\n" +
                $"EntityId={EntityId}\n" +
                $"PackedColor={PackedColor}\n" +
                $"Slot={Slot}\n" +
                $"OddAxis={OddAxis}\n" +
                $"GridPosition={(GridPosition.HasValue ? GridPosition.Value.ToString() : "NULL")}\n" +
                $"MirrorPlanes={(MirrorPlanes.HasValue ? MirrorPlanes.Value.ToString() : "NULL")}\n" +
                $"PackedColors={(PackedColors != null ? string.Join(",", PackedColors) : "NULL")}";
        }
    }

    public class Particle
    {
        public Color Color;
        public Vector3 RelativePosition;
        public Vector3 VelocityPerTick;
        public short Life;
        public float Radius;
        public float Angle;

        public Particle() { }
    }

    public struct DetectionInfo
    {
        public readonly IMyEntity Entity;
        public readonly Vector3D DetectionPoint;

        public DetectionInfo(IMyEntity entity, Vector3D detectionPoint)
        {
            Entity = entity;
            DetectionPoint = detectionPoint;
        }
    }
}