import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { CropperSettings } from 'ngx-img-cropper';
import { ImageCropConfig } from './image-crop-config.model';

@Component({
  selector: 'app-image-crop-dialog',
  template: `
    <div class="image-crop-dialog" >
      <ng-container *ngIf="!data || !data.image" >
        <p>Selecione um arquivo de imagem (.jpg, .png) para ajustar o tamanho e enviar</p>

        <div class="selection-btns" >
          <button class="btn-test" (click)="openUploadWindow()" >
            Selecionar Imagem
          </button>
          <button class="btn-outline" (click)="dismiss()" >
            Cancelar
          </button>
        </div>
      </ng-container>

      <img-cropper id="cropper"
        [hidden]="!data || !data.image"
        [image]="data"
        [settings]="cropperSettings"
      ></img-cropper>

      <ng-container *ngIf="data && data.image" >
        <div>
          <p class="final" >
            Imagem Final
            ({{dialogConfig.croppedWidth}}x{{dialogConfig.croppedHeight}})
          </p>
          <img
            [src]="data.image"
            [width]="cropperSettings.croppedWidth"
            [height]="cropperSettings.croppedHeight"
          />
        </div>

        <div class="selection-btns" >
          <button class="btn-test" (click)="confirmImage()" >
            Confirmar
          </button>
          <button class="btn-outline" (click)="dismiss()" >
            Cancelar
          </button>
        </div>
      </ng-container>
    </div>
  `,
  styleUrls: ['./image-crop.dialog.scss']
})
export class ImageCropDialogComponent implements OnInit {

  public data: any;
  public cropperSettings: CropperSettings;

  constructor(public dialogRef: MatDialogRef<ImageCropDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public dialogConfig: ImageCropConfig) { }

  ngOnInit() {
    this.cropperSettings = new CropperSettings();
    this.cropperSettings.width = this.dialogConfig.croppedWidth;
    this.cropperSettings.height = this.dialogConfig.croppedHeight;
    this.cropperSettings.croppedWidth = this.dialogConfig.croppedWidth;
    this.cropperSettings.croppedHeight = this.dialogConfig.croppedHeight;
    this.cropperSettings.canvasWidth = this.dialogConfig.canvasWidth || 300;
    this.cropperSettings.canvasHeight = this.dialogConfig.canvasHeight || 300;
    this.data = {};
  }

  public confirmImage(): void {
    this.dialogRef.close( this.data );
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public openUploadWindow(): void {
    (document.querySelector('#cropper > span > input') as HTMLElement).click();
  }

}
