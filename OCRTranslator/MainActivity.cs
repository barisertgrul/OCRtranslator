using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using static Android.Gms.Vision.Detector;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Android.Util;
using System.Text;
using Android.Content;
using Android.Provider;

namespace OCRTranslator
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = true)]
    public class MainActivity : Activity, ISurfaceHolderCallback, IProcessor
    {
        private SurfaceView surfaceView;
        private TextView textView;
        private CameraSource cameraSource;
        private ImageView capture;
        private LinearLayout mainLayout;
        private LinearLayout captureLayout;
        private ImageView back;
        private ImageView speak;
        private ImageView translate;
        private ImageView settings;
        private ImageView gallery;
        private ImageView selectedimage;
        private TextRecognizer textRecognizer;


        private const int RequestCameraPermissionID = 1001;
        public static readonly int PickImageID = 1000;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
           
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            surfaceView = FindViewById<SurfaceView>(Resource.Id.surface_view);
            textView = FindViewById<TextView>(Resource.Id.txt_view);
            capture = FindViewById<ImageView>(Resource.Id.imageView2);
            mainLayout = FindViewById<LinearLayout>(Resource.Id.mainmenulayout);
            captureLayout = FindViewById<LinearLayout>(Resource.Id.capturemenulayout);
            back = FindViewById<ImageView>(Resource.Id.imageView4);
            speak = FindViewById<ImageView>(Resource.Id.imageView5);
            translate = FindViewById<ImageView>(Resource.Id.imageView6);
            settings = FindViewById<ImageView>(Resource.Id.imageView3);
            gallery = FindViewById<ImageView>(Resource.Id.imageView1);
            selectedimage = FindViewById<ImageView>(Resource.Id.imageView7);



            capture.Click += (sender, e) =>
            {
                cameraSource.Stop();

                mainLayout.Visibility = ViewStates.Gone;
                captureLayout.Visibility = ViewStates.Visible;
                    textView.FocusableInTouchMode = true;

            };
            back.Click += (sender, e) =>
            {
                captureLayout.Visibility = ViewStates.Gone;
                selectedimage.Visibility = ViewStates.Gone;
                mainLayout.Visibility = ViewStates.Visible;
                surfaceView.Visibility = ViewStates.Visible;
                cameraSource.Start(surfaceView.Holder);
            };

            gallery.Click += (sender, e) =>
            {
                cameraSource.Stop();
                Intent galleryIntent = new Intent();
                galleryIntent.SetType("image/*");
                galleryIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(galleryIntent,"select"),PickImageID);

            };


            textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();
            if (!textRecognizer.IsOperational)
            {
                //todo:exception
            }
            else
            {
                cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(true)
                    .Build();

                surfaceView.Holder.AddCallback(this);
                textRecognizer.SetProcessor(this);
            }

        }
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
           
        }
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if(ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Android.Manifest.Permission.Camera

                }, RequestCameraPermissionID);
                return;
            }
            cameraSource.Start(surfaceView.Holder);
        }
       
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            cameraSource.Stop();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestCameraPermissionID:
                        if (grantResults[0] == Permission.Granted)
                        {
                            cameraSource.Start(surfaceView.Holder);
                        }
                    break;
                default:
                    break;
            }

        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            if(items.Size() != 0)
            {
                textView.Post(() =>
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for(int i = 0; i< items.Size(); i++)
                    {
                        stringBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                        stringBuilder.Append(" ");
                    }
                    textView.Text = stringBuilder.ToString();
                });
            }
        }

        public void Release()
        {
            
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(requestCode==PickImageID && resultCode== Result.Ok && data!= null)
            {
                Android.Net.Uri uri = data.Data;
                cameraSource.Stop();
                surfaceView.Visibility = ViewStates.Gone;
                selectedimage.Visibility = ViewStates.Visible;
                mainLayout.Visibility = ViewStates.Gone;
                captureLayout.Visibility = ViewStates.Visible;
                selectedimage.SetImageURI(uri);

                Bitmap currentImage = MediaStore.Images.Media.GetBitmap(this.ContentResolver, uri);
                selectedimage.SetImageBitmap(currentImage);
                Frame frame = new Frame.Builder().SetBitmap(currentImage).Build();
                SparseArray items = textRecognizer.Detect(frame);
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < items.Size(); i++)
                {
                    stringBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                    stringBuilder.Append(" ");
                }
                textView.Text = stringBuilder.ToString();


            }


        }
    }
}

