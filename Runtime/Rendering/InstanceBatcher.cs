using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Til3map
{
    public class InstanceBatcher<T>
    {
        // Related to Unity DrawMeshInstanced.
        private const int BatchSize = 1023;
        private const float BatchSizeF = (float) BatchSize;

        public List<T> Instances { get; private set; }
        public List<T[]> Batches { get; private set; }

        public InstanceBatcher(List<T> instances, bool updateBatches = true)
        {
            SetInstances(instances, updateBatches);
        }

        public void SetInstances(List<T> instances, bool updateBatches = true)
        {
            Instances = instances;
            if (updateBatches)
            {
                UpdateBatches();
            }
        }

        public void UpdateBatches()
        {
            int batchCount = Mathf.CeilToInt(Instances.Count / BatchSizeF);
            if (Batches == null)
            {
                Batches = new List<T[]>(batchCount);
            }
            else
            {
                Batches.Clear();
            }

            int remainingInstances = Instances.Count;
            for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                int batchSize = Mathf.Min(BatchSize, remainingInstances);
                var batch = new T[batchSize];

                for (int i = 0; i < batchSize; i++)
                {
                    batch[i] = Instances[batchIndex * BatchSize + i];
                }

                Batches.Add(batch);
                remainingInstances -= BatchSize;
            }
        }
    }
}

