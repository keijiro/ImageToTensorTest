using UnityEngine;

class ImageScaler : System.IDisposable
{
    #region Public interface

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

    const float MaxAspectGap = 0.35f;

    #endregion

    #region Scaler implementation

    public void RunScaler(Texture2D source)
    {
        var aspect = (float)source.width / source.height;
        var (sx, sy) = (1.0f, 1.0f);

        if (aspect > 1)
            sy = Mathf.Max(aspect - MaxAspectGap, 1);
        else
            sx = Mathf.Max(1 / aspect - MaxAspectGap, 1);

        _compute.SetTexture(0, "_sc_input", source);
        _compute.SetInt("_sc_width", _width);
        _compute.SetVector("_sc_scale", new Vector2(sx, sy));
        _compute.SetBuffer(0, "_sc_output", _squarified);
        _compute.Dispatch(0, _width / 8, _width / 8, 1);
    }

    #endregion
}
