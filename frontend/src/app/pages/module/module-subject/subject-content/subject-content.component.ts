import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ForumQuestion } from 'src/app/models/forum.model';
import { Subject } from 'src/app/models/subject.model';

@Component({
  selector: 'app-subject-content',
  templateUrl: './subject-content.component.html',
  styleUrls: ['./subject-content.component.scss']
})
export class SubjectContentComponent {

  @Input() readonly moduleId: string;
  @Input() readonly subject: Subject;
  @Input() readonly index: number = 0;
  @Input() readonly subjectProgress: any;
  @Input() readonly hasFinishedRequirements: boolean = true;

  constructor(private _router: Router) { }

  public goToContent(): void {
    localStorage.setItem('contents', JSON.stringify(this.subject.contents));
    localStorage.setItem('contents-hasQuestions', JSON.stringify(this.subject.hasQuestions));
    localStorage.setItem('subjectProgress', JSON.stringify(this.subjectProgress));
    localStorage.setItem('hasFinishedRequirements', this.hasFinishedRequirements.toString());
    const localForumQuestion: ForumQuestion = JSON.parse(localStorage.getItem('forumQuestionDialog'));
    localForumQuestion.subjectId = this.subject.id;
    localForumQuestion.subjectName = this.subject.title;
    localForumQuestion.contentId = this.subject.contents[this.index].id;
    localForumQuestion.contentName = this.subject.contents[this.index].title;
    localStorage.setItem('forumQuestionDialog', JSON.stringify(localForumQuestion));
    this._router.navigate(['/modulo/' + this.moduleId + '/' + this.subject.id + '/' + this.index]);
  }

}
