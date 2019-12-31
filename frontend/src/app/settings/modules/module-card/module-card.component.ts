import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ModulePreview } from '../../../models/previews/module.interface';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-settings-module-card',
  template: `
    <div class="module-card" >
      <img class="main-img"
        [src]="module.imageUrl"
        onError="this.src='./assets/img/420x210-placeholder.png'"
      />

      <div class="draft-tag" [ngClass]="{ 'draft': module.isDraft }" >
        {{ module.isDraft ? 'RASCUNHO': 'PUBLICADO' }}
      </div>

      <div class="preview" >
        <div>
          <h3>
            {{ module.title }}
          </h3>
          <p>
            {{ module.instructor }}<br>
            <small *ngIf="isAdmin" >
              Id: {{ module.id }}
            </small>
          </p>
        </div>
        <p class="content" >
          {{ module.subjects ? module.subjects.length : 0 }} ASSUNTOS -
          {{ getContentsAmount() }} CONTEÚDOS
        </p>
      </div>

      <div class="edit">
        <img src="./assets/img/edit.png" (click)="editModule.emit(module)" />
        <img src="./assets/img/icon_trilha.png" (click)="cloneModule.emit(module)" />
        <img src="./assets/img/trash.png" (click)="deleteModule.emit(module)" />
      </div>
    </div>
  `,
  styleUrls: ['./module-card.component.scss']
})
export class SettingsModuleCardComponent {

  @Input() module: ModulePreview;
  @Output() editModule = new EventEmitter<ModulePreview>();
  @Output() deleteModule = new EventEmitter<ModulePreview>();
  @Output() cloneModule = new EventEmitter<ModulePreview>();

  public isAdmin: boolean = false;

  constructor(
    private _authService: AuthService
  ) {
    const user = this._authService.getLoggedUser();
    this.isAdmin = user && user.role && user.role === 'Admin';
  }

  public getContentsAmount(): number {
    if (!this.module.subjects) return 0;

    return this.module.subjects.reduce((sum, sub) => {
      return sub.contents ? sum + sub.contents.length : sum;
    }, 0);
  }

}
