import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { SuggestionEventSelectComponent } from '../../manage-suggestion/event-select/event-select.component';
import { eventsMock } from '../mocks';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material';

const dbEvent = eventsMock[0];

describe('[Integration] SuggestionEventSelectComponent', () => {

  let fixture: ComponentFixture<SuggestionEventSelectComponent>;
  let component: SuggestionEventSelectComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        FormsModule,
        MatCheckboxModule
      ],
      declarations: [
        SuggestionEventSelectComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SuggestionEventSelectComponent);
      component = fixture.componentInstance;
      component.setEvent = dbEvent;
    });
  }));

  it('should display the event image', async () => {
    fixture.detectChanges();
    const imgElement = fixture.nativeElement.querySelector('img.main-img');
    expect(imgElement.src).toContain(dbEvent.imageUrl);
  });

  it('should display the event title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.preview h3');
    expect(titleElement.textContent).toContain(dbEvent.title);
  });
});
