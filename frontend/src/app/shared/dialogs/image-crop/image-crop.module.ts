import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ImageCropDialogComponent } from './image-crop.dialog';
import { ImageCropperModule } from 'ngx-img-cropper';

@NgModule({
  declarations: [
    ImageCropDialogComponent
  ],
  imports: [
    BrowserModule,
    ImageCropperModule
  ],
  entryComponents: [
    ImageCropDialogComponent
  ]
})
export class ImageCropModule { }
