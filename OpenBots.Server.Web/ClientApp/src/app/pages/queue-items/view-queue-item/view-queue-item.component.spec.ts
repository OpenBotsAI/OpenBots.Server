import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewQueueItemComponent } from './view-queue-item.component';

describe('ViewQueueItemComponent', () => {
  let component: ViewQueueItemComponent;
  let fixture: ComponentFixture<ViewQueueItemComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewQueueItemComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewQueueItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
