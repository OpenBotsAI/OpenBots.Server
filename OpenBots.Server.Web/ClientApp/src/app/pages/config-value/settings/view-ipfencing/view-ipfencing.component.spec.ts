import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewIPFencingComponent } from './view-ipfencing.component';

describe('ViewIPFencingComponent', () => {
  let component: ViewIPFencingComponent;
  let fixture: ComponentFixture<ViewIPFencingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewIPFencingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewIPFencingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
