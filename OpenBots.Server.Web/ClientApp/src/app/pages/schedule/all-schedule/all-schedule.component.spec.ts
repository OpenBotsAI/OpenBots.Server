import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllScheduleComponent } from './all-schedule.component';

describe('AllScheduleComponent', () => {
  let component: AllScheduleComponent;
  let fixture: ComponentFixture<AllScheduleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllScheduleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllScheduleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
