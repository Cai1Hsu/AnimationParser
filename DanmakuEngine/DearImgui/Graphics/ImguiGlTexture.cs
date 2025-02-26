#pragma warning disable IDE1006

using Silk.NET.OpenGL;

using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;

namespace DanmakuEngine.DearImgui.Graphics;

internal class ImguiFontTexture : IDisposable
{
    public const SizedInternalFormat Srgb8Alpha8 = (SizedInternalFormat)GLEnum.Srgb8Alpha8;
    public const SizedInternalFormat Rgb32F = (SizedInternalFormat)GLEnum.Rgb32f;

    public const GLEnum MaxTextureMaxAnisotropy = (GLEnum)0x84FF;

    public static float? MaxAniso;
    private readonly GL _gl;
    public readonly uint GlTexture;
    public readonly uint Width, Height;
    public readonly uint MipmapLevels;
    public readonly SizedInternalFormat InternalFormat;

    public unsafe ImguiFontTexture(GL gl, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
    {
        _gl = gl;
        MaxAniso ??= gl.GetFloat(MaxTextureMaxAnisotropy);
        Width = (uint)width;
        Height = (uint)height;
        InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;
        MipmapLevels = (uint)(generateMipmaps == false ? 1 : (int)Math.Floor(Math.Log(Math.Max(Width, Height), 2)));

        GlTexture = _gl.GenTexture();
        Bind();

        PixelFormat pxFormat = PixelFormat.Bgra;

        _gl.TexStorage2D(GLEnum.Texture2D, MipmapLevels, InternalFormat, Width, Height);
        _gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, Width, Height, pxFormat, PixelType.UnsignedByte, (void*)data);

        if (generateMipmaps)
            _gl.GenerateTextureMipmap(GlTexture);

        SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
        SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);

        var level = MipmapLevels - 1;
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMaxLevel, &level);
    }

    public void Bind()
    {
        _gl.BindTexture(GLEnum.Texture2D, GlTexture);
    }

    public void SetMinFilter(TextureMinFilter filter)
    {
        unsafe
        {
            _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, (int*)&filter);
        }
    }

    public void SetMagFilter(TextureMagFilter filter)
    {
        unsafe
        {
            _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, (int*)&filter);
        }
    }

    public void SetAnisotropy(float level)
    {
        const TextureParameterName textureMaxAnisotropy = (TextureParameterName)0x84FE;
        _gl.TexParameter(GLEnum.Texture2D, (GLEnum)textureMaxAnisotropy,
            Math.Clamp(level, 1, MaxAniso.GetValueOrDefault()));
    }

    public void SetLod(int @base, int min, int max)
    {
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureLodBias, in @base);
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinLod, in min);
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMaxLod, in max);
    }

    public void SetWrap(TextureCoordinate coord, TextureWrapMode mode)
    {
        unsafe
        {
            _gl.TexParameterI(GLEnum.Texture2D, (TextureParameterName)coord, (int*)&mode);
        }
    }

    public void Dispose()
    {
        _gl.DeleteTexture(GlTexture);
    }

    public enum TextureCoordinate
    {
        S = TextureParameterName.TextureWrapS,
        T = TextureParameterName.TextureWrapT,
        R = TextureParameterName.TextureWrapR
    }
}

