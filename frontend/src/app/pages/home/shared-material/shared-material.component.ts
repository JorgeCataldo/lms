import { Component, Input } from '@angular/core';
import { SharedMaterialPreview } from '../../../models/previews/shared-material.interface';

@Component({
  selector: 'app-shared-material',
  template: `
    <div class="shared-material" >
      <div class="owner" >
        <p>
          <small>Compartilhado por</small><br>
          {{ material.owner }}
        </p>
        <img src="./assets/img/shared-content.png" />
      </div>
      <div class="content" >
        <p class="title" >{{ material.title }}</p>

        <p>{{ material.excerpt }}</p>

        <button>
          Abrir Site
        </button>
      </div>
    </div>`,
  styleUrls: ['./shared-material.component.scss']
})
export class SharedMaterialComponent {

  @Input() material: SharedMaterialPreview;

}
