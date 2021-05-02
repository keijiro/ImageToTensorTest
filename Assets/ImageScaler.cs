using UnityEngine;

class ImageScaler : System.IDisposable
{
    #region Public interface

    public Vector2 ScaleFactor { get; private set; }

    public ComputeBuffer SquarifiedTensor => _squarified;

    public ImageScaler(int width, ComputeShader compute)
      => (_width, _compute, _squarified) =
           (width, compute,
            new ComputeBuffer(width * width, sizeof(float) * 3));

    public void Dispose()
      => _squarified.Dispose();

    public void ProcessImage(Texture2D source)
      => RunScaler(source);

    #endregion

    #region Private objects

    int _width;
    ComputeShader _compute;
    ComputeBuffer _squarified;

    const float MaxAspectGap = 0.75f;

    #endregion

    #region Scaler implementation

    public void RunScaler(Texture2D source)
    {
        (float w, float h) = (source.width, source.height);

        if (w > h)
            ScaleFactor = new Vector2(1, Mathf.Max(w / h * MaxAspectGap, 1));
        else
            ScaleFactor = new Vector2(Mathf.Max(h / w * MaxAspectGap, 1), 1);

        _compute.SetTexture(0, "_sc_input", source);
        _compute.SetInt("_sc_width", _width);
        _compute.SetVector("_sc_scale", ScaleFactor);
        _compute.SetBuffer(0, "_sc_output", _squarified);
        _compute.Dispatch(0, _width / 8, _width / 8, 1);
    }

    #endregion
}
