import { Component, Output, EventEmitter, Input, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Requirement } from '../../models/new-requirement.model';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Module } from '../../../../../models/module.model';
import { Level } from '../../../../../models/shared/level.interface';

@Component({
  selector: 'app-new-module-requirements',
  templateUrl: './requirements.component.html',
  styleUrls: ['../new-module-steps.scss', './requirements.component.scss']
})
export class NewModuleRequirementsComponent extends NotificationClass implements OnInit {

  @Input() readonly module: Module;
  @Input() readonly levels: Array<Level>;
  @Output() setRequirements = new EventEmitter<Array<Array<Requirement>>>();

  public requirements: Array<Array<Requirement>> = [ [], [] ];

  public requiredModules: Array<Requirement> = [];
  public newModule = new Requirement('', 0);
  public editing: boolean = false;

  public optionalModules: Array<Requirement> = [];
  public newOptionalModule = new Requirement('', 0);
  public editingOptional: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    if (this.module && this.module.requirements) {
      this.requirements = [
        this.module.requirements.filter(r => !r.optional),
        this.module.requirements.filter(r => r.optional)
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
