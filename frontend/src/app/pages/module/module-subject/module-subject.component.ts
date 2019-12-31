import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from '../../../models/subject.model';
import { UtilService } from '../../../shared/services/util.service';
import { ActionInfo, ActionInfoPageEnum, ActionInfoTypeEnum } from 'src/app/shared/directives/save-action/action-info.interface';
import { ForumQuestion } from 'src/app/models/forum.model';
import { ContentModulesService } from '../../_services/modules.service';

@Component({
  selector: 'app-module-subject',
  templateUrl: './module-subject.component.html',
  styleUrls: ['./module-subject.component.scss']
})
export class ModuleSubjectComponent implements OnInit {

  @Input() readonly moduleId: string;
  @Input() readonly subject: Subject;
  @Input() readonly subjectProgress: any;
  @Input() readonly levels = { };
  @Input() readonly hasFinishedRequirements: boolean = true;

  public expanded: boolean = false;
  public contents = [{}, {}, {}];

  constructor(
    private _router: Router,
    private _utilService: UtilService,
    private _moduleService: ContentModulesService
  ) { }

  ngOnInit() {
    this.subject.duration = this.subject.contents.reduce((sum, c) => sum + c.duration, 0);
    const allContentStr = localStorage.getItem('allContents');
    if (!allContentStr || allContentStr.trim() === '') {
        this._moduleService.getAllContent(this.moduleId).subscribe(res => {
          localStorage.setItem('allContents', JSON.stringify(res.data));
        });
      }
    }

  public reachedMaxLevel(): boolean {
    return this.levels && this.levels[this.subjectProgress.level] === undefined;
  }

  public getSubjectDuration(): string {
    return this._utilService.formatSecondsToMinutes(this.subject.duration);
  }

  public goToExam(): void {
    this._router.navigate([ '/avaliacao/' + this.moduleId + '/' + this.subject.id ]);
  }

  public goToContent(): void {
    if (this.subject && this.subject.contents && this.subject.contents.length > 0) {
      localStorage.setItem('contents', JSON.stringify(this.subject.contents));
      localStorage.setItem('contents-hasQuestions', JSON.stringify(this.subject.hasQuestions));
      localStorage.setItem('subjectProgress', JSON.stringify(this.subjectProgress));
      localStorage.setItem('hasFinishedRequirements', this.hasFinishedRequirements.toString());
      const localForumQuestion: ForumQuestion = JSON.parse(localStorage.getItem('forumQuestionDialog'));
      localForumQuestion.subjectId = this.subject.id;
      localForumQuestion.subjectName = this.subject.title;
      localForumQuestion.contentId = this.subject.contents[0].id;
      localForumQuestion.contentName = this.subject.contents[0].title;
      localStorage.setItem('forumQuestionDialog', JSON.stringify(localForumQuestion));
      this._router.navigate(['/modulo/' + this.moduleId + '/' + this.subject.id + '/0' ]);
    }
  }

  public getActionInfo(description: string): ActionInfo {
    console.log(this.subjectProgress);
    console.log(this.levels);
    return {
      page: ActionInfoPageEnum.Module,
      description: description,
      type: ActionInfoTypeEnum.Click,
      moduleId: this.moduleId,
      subjectId: this.subject.id
    };
  }

}
