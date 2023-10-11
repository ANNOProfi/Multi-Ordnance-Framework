using UnityEngine;
using Verse;

namespace ShieldbreakerPermits
{
    public class SP_BombardmentProjectile : IExposable
    {
        public int LifeTime
        {
            get
            {
                return this.lifeTime;
            }
        }

        public SP_BombardmentProjectile()
        {
        }

        public SP_BombardmentProjectile(int lifeTime, IntVec3 targetCell)
        {
            this.lifeTime = lifeTime;
            this.maxLifeTime = lifeTime;
            this.targetCell = targetCell;
        }

        public void Tick()
        {
            this.lifeTime--;
        }

        public void Draw(Material material)
        {
            if (this.lifeTime > 0)
            {
                Vector3 pos = this.targetCell.ToVector3() + Vector3.forward * Mathf.Lerp(60f, 0f, 1f - (float)this.lifeTime / (float)this.maxLifeTime);
                pos.z += 1.25f;
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(pos, Quaternion.Euler(0f, 180f, 0f), new Vector3(2.5f, 1f, 2.5f));
                Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.lifeTime, "lifeTime", 0, false);
            Scribe_Values.Look<int>(ref this.maxLifeTime, "maxLifeTime", 0, false);
            Scribe_Values.Look<IntVec3>(ref this.targetCell, "targetCell", default(IntVec3), false);
        }

        private int lifeTime;

        private int maxLifeTime;

        public IntVec3 targetCell;

        private const float StartZ = 60f;

        private const float Scale = 2.5f;

        private const float Angle = 180f;
    }
}