import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllSystemEventsComponent } from './all-system-events.component';

describe('AllSystemEventsComponent', () => {
  let component: AllSystemEventsComponent;
  let fixture: ComponentFixture<AllSystemEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllSystemEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllSystemEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
