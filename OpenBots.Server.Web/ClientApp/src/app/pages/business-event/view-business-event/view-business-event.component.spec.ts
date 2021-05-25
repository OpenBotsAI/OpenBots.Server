import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewBusinessEventComponent } from './view-business-event.component';

describe('ViewBusinessEventComponent', () => {
  let component: ViewBusinessEventComponent;
  let fixture: ComponentFixture<ViewBusinessEventComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewBusinessEventComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewBusinessEventComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
