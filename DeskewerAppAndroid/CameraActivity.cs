using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Hardware;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DeskewerAppAndroid
{
    [Activity(Label = "CameraActivity")]
    public class CameraActivity : Activity, TextureView.ISurfaceTextureListener
    {
        Camera _camera;
        TextureView _textureView;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _textureView = new TextureView(this);
            _textureView.SurfaceTextureListener = this;

            SetContentView(_textureView);
        }

        public void OnSurfaceTextureAvailable(
                Android.Graphics.SurfaceTexture surface, int w, int h)
        {
            _camera = Camera.Open();

            _textureView.LayoutParameters =
                   new FrameLayout.LayoutParams(w, h);

            try
            {
                _camera.SetPreviewTexture(surface);
                _camera.StartPreview();

            }
            catch (Java.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool OnSurfaceTextureDestroyed(
                Android.Graphics.SurfaceTexture surface)
        {
            _camera.StopPreview();
            _camera.Release();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(
                Android.Graphics.SurfaceTexture surface, int a, int b)
        {
        }

        public void OnSurfaceTextureUpdated(
                Android.Graphics.SurfaceTexture surface)
        {
        }
    }
}