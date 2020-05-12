﻿using System.Collections.Generic;
using Digi.PaintGun.Utilities;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Digi.PaintGun.Features.Palette
{
    public class Painting : ModComponent
    {
        HashSet<MyCubeGrid> gridsInSystemCache = new HashSet<MyCubeGrid>();

        public Painting(PaintGunMod main) : base(main)
        {
        }

        protected override void RegisterComponent()
        {
        }

        protected override void UnregisterComponent()
        {
        }

        public void PaintBlock(IMyCubeGrid grid, Vector3I gridPosition, PaintMaterial paint, ulong originalSenderSteamId)
        {
            // NOTE getting a MySlimBlock and sending it straight to arguments avoids getting prohibited errors.
            var gridInternal = (MyCubeGrid)grid;
            gridInternal.ChangeColorAndSkin(gridInternal.GetCubeBlock(gridPosition), paint.ColorMask, paint.Skin);
        }

        public void ReplaceColorInGrid(IMyCubeGrid grid, BlockMaterial oldPaint, PaintMaterial paint, bool includeSubgrids, ulong originalSenderSteamId)
        {
            var gridInternal = (MyCubeGrid)grid;
            gridsInSystemCache.Clear();

            if(includeSubgrids)
                Utils.GetShipSubgrids(gridInternal, gridsInSystemCache);
            else
                gridsInSystemCache.Add(gridInternal);

            int affected = 0;

            foreach(var subgrid in gridsInSystemCache)
            {
                foreach(IMySlimBlock slim in subgrid.CubeBlocks)
                {
                    var blockMaterial = new BlockMaterial(slim);

                    if(paint.ColorMask.HasValue && !Utils.ColorMaskEquals(blockMaterial.ColorMask, oldPaint.ColorMask))
                        continue;

                    if(paint.Skin.HasValue && blockMaterial.Skin != oldPaint.Skin)
                        continue;

                    PaintBlock(subgrid, slim.Position, paint, originalSenderSteamId);
                    affected++;
                }
            }

            if(originalSenderSteamId == MyAPIGateway.Multiplayer.MyId)
            {
                Main.Notifications.Show(2, $"Replaced color for {affected.ToString()} blocks.", MyFontEnum.White, 2000);
            }
        }

        #region Symmetry
        public void PaintBlockSymmetry(IMyCubeGrid grid, Vector3I gridPosition, PaintMaterial paint, MirrorPlanes mirrorPlanes, OddAxis odd, ulong originalSenderSteamId)
        {
            PaintBlock(grid, gridPosition, paint, originalSenderSteamId);

            bool oddX = (odd & OddAxis.X) == OddAxis.X;
            bool oddY = (odd & OddAxis.Y) == OddAxis.Y;
            bool oddZ = (odd & OddAxis.Z) == OddAxis.Z;

            var alreadyMirrored = Caches.AlreadyMirrored;
            alreadyMirrored.Clear();

            var mirrorX = MirrorPaint(grid, 0, mirrorPlanes, oddX, gridPosition, paint, alreadyMirrored, originalSenderSteamId); // X
            var mirrorY = MirrorPaint(grid, 1, mirrorPlanes, oddY, gridPosition, paint, alreadyMirrored, originalSenderSteamId); // Y
            var mirrorZ = MirrorPaint(grid, 2, mirrorPlanes, oddZ, gridPosition, paint, alreadyMirrored, originalSenderSteamId); // Z
            Vector3I? mirrorYZ = null;

            if(mirrorX.HasValue && mirrorPlanes.Y > int.MinValue) // XY
                MirrorPaint(grid, 1, mirrorPlanes, oddY, mirrorX.Value, paint, alreadyMirrored, originalSenderSteamId);

            if(mirrorX.HasValue && mirrorPlanes.Z > int.MinValue) // XZ
                MirrorPaint(grid, 2, mirrorPlanes, oddZ, mirrorX.Value, paint, alreadyMirrored, originalSenderSteamId);

            if(mirrorY.HasValue && mirrorPlanes.Z > int.MinValue) // YZ
                mirrorYZ = MirrorPaint(grid, 2, mirrorPlanes, oddZ, mirrorY.Value, paint, alreadyMirrored, originalSenderSteamId);

            if(mirrorPlanes.X > int.MinValue && mirrorYZ.HasValue) // XYZ
                MirrorPaint(grid, 0, mirrorPlanes, oddX, mirrorYZ.Value, paint, alreadyMirrored, originalSenderSteamId);
        }

        Vector3I? MirrorPaint(IMyCubeGrid grid, int axis, MirrorPlanes mirrorPlanes, bool odd, Vector3I originalPosition, PaintMaterial paint, List<Vector3I> alreadyMirrored, ulong originalSenderSteamId)
        {
            Vector3I? mirrorPosition = null;

            switch(axis)
            {
                case 0:
                    if(mirrorPlanes.X.HasValue)
                        mirrorPosition = originalPosition + new Vector3I(((mirrorPlanes.X.Value - originalPosition.X) * 2) - (odd ? 1 : 0), 0, 0);
                    break;

                case 1:
                    if(mirrorPlanes.Y.HasValue)
                        mirrorPosition = originalPosition + new Vector3I(0, ((mirrorPlanes.Y.Value - originalPosition.Y) * 2) - (odd ? 1 : 0), 0);
                    break;

                case 2:
                    if(mirrorPlanes.Z.HasValue)
                        mirrorPosition = originalPosition + new Vector3I(0, 0, ((mirrorPlanes.Z.Value - originalPosition.Z) * 2) + (odd ? 1 : 0)); // reversed on odd
                    break;
            }

            if(mirrorPosition.HasValue && originalPosition != mirrorPosition.Value && !alreadyMirrored.Contains(mirrorPosition.Value) && grid.CubeExists(mirrorPosition.Value))
            {
                alreadyMirrored.Add(mirrorPosition.Value);
                PaintBlock(grid, mirrorPosition.Value, paint, originalSenderSteamId);
            }

            return mirrorPosition;
        }
        #endregion Symmetry
    }
}