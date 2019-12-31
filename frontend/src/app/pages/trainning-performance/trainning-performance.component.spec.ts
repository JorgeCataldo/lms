import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainningPerformanceComponent } from './trainning-performance.component';

describe('TrainningPerformanceComponent', () => {
  let component: TrainningPerformanceComponent;
  let fixture: ComponentFixture<TrainningPerformanceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TrainningPerformanceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TrainningPerformanceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
