using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CsharpVoxReader.Chunks {
    public class TransformNode : Chunk {
        public const string ID = "nTRN";

        internal override string Id {
            get { return ID; }
        }

        internal override int Read(BinaryReader br, IVoxLoader loader) {
            int readSize = base.Read(br, loader);

            Int32 id = br.ReadInt32();
            // readSize += sizeof(Int32);
            Dictionary<string, byte[]> attributes = GenericsReader.ReadDict(br, ref readSize);

            attributes.TryGetName(out var name);

            Int32 childNodeId = br.ReadInt32();
            Int32 reservedId = br.ReadInt32();
            Int32 layerId = br.ReadInt32();
            Int32 numOfFrames = br.ReadInt32();
            if (numOfFrames <= 0) {
                // this is invalid
                
            }
            // UnityEngine.Debug.Log($"{id},{attributes.Count},{childNodeId},{reservedId},{layerId},{numOfFrames}");

            readSize += sizeof(Int32) * 5;

            Dictionary<string, byte[]>[] framesAttributes = new Dictionary<string, byte[]>[numOfFrames];

            TransformNodeFrameData[] transformNodeFrameData = new TransformNodeFrameData[numOfFrames];
            for (int fnum = 0; fnum < numOfFrames; fnum++) {
                framesAttributes[fnum] = GenericsReader.ReadDict(br, ref readSize);
                // UnityEngine.Debug.Log(framesAttributes[fnum].Count);
                // UnityEngine.Debug.Log(framesAttributes[fnum].Select(kvp => $"{kvp.Key}: {kvp.Value.ToString()}")
                    // .Aggregate((a, b) => $"{a}, {b}"));/// todo its empty!
                // byte rot = framesAttributes[fnum]["_r"][0];
                // byte[] tra = framesAttributes[fnum]["_t"];
                // byte[] fi = framesAttributes[fnum]["_f"];

                // transformNodeFrameData[fnum].rotationMatrix = GenericsReader.ReadRotation(rot);
                // transformNodeFrameData[fnum].translationVector = new int[] { 0, 0, 0 };
                // const int isize = sizeof(Int32);
                // transformNodeFrameData[fnum].translationVector[0] = BitConverter.ToInt32(tra.AsSpan(0, isize));
                // transformNodeFrameData[fnum].translationVector[1] = BitConverter.ToInt32(tra.AsSpan(isize * 1, isize));
                // transformNodeFrameData[fnum].translationVector[2] = BitConverter.ToInt32(tra.AsSpan(isize * 2, isize));
                // transformNodeFrameData[fnum].frameIndex = BitConverter.ToInt32(fi, 0);
                // readSize += sizeof(Int32) * 4;
                //   // TODO: Add frame info
            }
            // // TODO: Notify the IVoxLoader of the transform node
            loader.NewTransformNode(id, childNodeId, layerId, name, framesAttributes, transformNodeFrameData);
            return readSize;
        }
    }
}
