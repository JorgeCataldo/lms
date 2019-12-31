import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { subjectsMock } from '../mocks';
import { SubjectContentComponent } from '../../module-subject/subject-content/subject-content.component';
import { Content } from 'src/app/models/content.model';
import { ForumQuestion } from 'src/app/models/forum.model';

const subject = subjectsMock[0];

describe('[Integration] RequiredModuleComponent', () => {

  let fixture: ComponentFixture<SubjectContentComponent>;
  let component: SubjectContentComponent;
  let router: Router;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        RouterTestingModule.withRoutes([])
      ],
      declarations: [
        SubjectContentComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SubjectContentComponent);
      component = fixture.componentInstance;
      router = TestBed.get(Router);

      (component.moduleId as any) = '5c6d5a9d6484113c901f7f29';
      (component.subject as any) = subject;
      localStorage.setItem('forumQuestionDialog', JSON.stringify({}));
    });
  }));

  it('should display the content title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.header p.title');
    expect(titleElement.textContent).toContain(subject.contents[0].title);
  });

  it('should display the content excerpt', async () => {
    fixture.detectChanges();
    const descElement = fixture.nativeElement.querySelector('p.description');
    expect(descElement.textContent).toContain(subject.contents[0].excerpt);
  });

  it('should store the contents info in localstorage', async () => {
    spyOn(router, 'navigate');

    triggerClick();

    const contents = JSON.parse( localStorage.getItem('contents') ) as Array<Content>;
    const hasQuestions = localStorage.getItem('contents-hasQuestions');
    expect(contents.length).toBe(1);
    expect(contents[0].id).toBe('5c6d5e0b6484113c901f7f31');
    expect(hasQuestions).toBe('false');
  });

  it('should store the forum question info in localstorage', async () => {
    spyOn(router, 'navigate');

    triggerClick();

    const question = JSON.parse( localStorage.getItem('forumQuestionDialog') ) as ForumQuestion;
    expect(question.subjectId).toBe(subject.id);
    expect(question.subjectName).toBe(subject.title);
    expect(question.contentId).toBe(subject.contents[0].id);
    expect(question.contentName).toBe(subject.contents[0].title);
  });

  it('should navigate to the content page', async () => {
    const routerSpy = spyOn(router, 'navigate');

    triggerClick();

    expect(routerSpy).toHaveBeenCalledWith(
      [ '/modulo/' + component.moduleId + '/' + component.subject.id + '/' + component.index ]
    );
  });

  function triggerClick() {
    const editTestImage = fixture.nativeElement.querySelector('div.subject-content');
    editTestImage.dispatchEvent(new Event('click'));
    fixture.detectChanges();
  }

});
