using UnityEngine.UI;
public class CKEmptyRaycast : MaskableGraphic
{
    protected CKEmptyRaycast()
    {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
