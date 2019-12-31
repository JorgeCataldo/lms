import { MatSnackBar, MatDialog } from '@angular/material';
import { AbstractControl } from '@angular/forms';
import { SharedService } from '../services/shared.service';
import { NotificationClass } from './notification';
import { ImageCropDialogComponent } from '../dialogs/image-crop/image-crop.dialog';
import { UploadResource } from '../../models/shared/upload-resource.interface';

export class ImageUploadClass extends NotificationClass {

  constructor(
    protected _snackBar: MatSnackBar,
    protected _matDialog: MatDialog,
    protected _sharedService: SharedService
  ) {
    super(_snackBar);
  }

  public uploadImage(imageWidth: number, imageHeight: number, formControl: AbstractControl) {
    const dialogRef = this._matDialog.open(ImageCropDialogComponent, {
      width: window.innerWidth < 768 ? '100vw' : '50vw',
      data: {
        'canvasWidth': window.innerWidth < 768 ? 250 : 300,
        'canvasHeight': window.innerWidth < 768 ? 250 : 300,
        'croppedWidth': imageWidth,
        'croppedHeight': imageHeight
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.image) {
        this._uploadImage(
          result.image,
          formControl
        );
      }
    });
  }

  private _uploadImage(image: string, formControl: AbstractControl) {
    const resource: UploadResource = {
      data: image,
      filename: 'module.png',
      resource: 'module'
    };

    this._sharedService.uploadImage(resource).subscribe((response) => {
      formControl.setValue(response.data.imageUrl);
    }, () => {
      this.notify('Ocorreu um erro ao enviar a imagem, por favor tente novamente mais tarde');
    });
  }

}
