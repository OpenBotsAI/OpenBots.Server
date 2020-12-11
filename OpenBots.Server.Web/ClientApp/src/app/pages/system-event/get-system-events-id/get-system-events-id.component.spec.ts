import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetSystemEventsIdComponent } from './get-system-events-id.component';

describe('GetSystemEventsIdComponent', () => {
  let component: GetSystemEventsIdComponent;
  let fixture: ComponentFixture<GetSystemEventsIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetSystemEventsIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetSystemEventsIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
