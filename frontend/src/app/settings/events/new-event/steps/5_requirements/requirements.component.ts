import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Requirement } from '../../../../modules/new-module/models/new-requirement.model';
import { Event } from '../../../../../models/event.model';
import { Level } from '../../../../../models/shared/level.interface';

@Component({
  selector: 'app-new-event-requirements',
  template: `
    <div class="step" >
      <h2>Requisitos</h2>

      <app-requirements-config
        [requirements]="requirements"
        [levels]="levels"
        (setRequirements)="updateRequirements($event)"
      ></app-requirements-config>
    </div>`,
  styleUrls: ['../new-event-steps.scss']
})
export class NewEventRequirementsComponent extends NotificationClass implements OnInit {

  @Input() readonly event: Event;
  @Input() readonly levels: Array<Level>;
  @Output() setRequirements = new EventEmitter<Array<Array<Requirement>>>();

  public requirements: Array<Array<Requirement>> = [ [], [] ];

  public requiredModules: Array<Requirement> = [];
  public newModule = new Requirement('', 0);
  public editing: boolean = false;

  public optionalModules: Array<Requirement> = [];
  public newOptionalModule = new Requirement('', 0);
  public editingOptional: boolean = false;

  constructor(protected _snackBar: MatSnackBar) {
    super(_snackBar);
  }

  ngOnInit() {
    if (this.event && this.event.requirements) {
      this.requirements = [
        this.event.requirements.filter(r => !r.optional),
        this.event.requirements.filter(r => r.optional)
      ];
    }
  }

  public updateRequirements(requirements: Array<Array<Requirement>>): void {
    this.requirements = requirements;
  }

  public nextStep(): void {
    this.setRequirements.emit( this.requirements );
  }

}
