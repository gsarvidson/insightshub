import {
  Component, ElementRef, OnChanges, OnDestroy, ViewChild,
  input, afterNextRender,
} from '@angular/core';
import * as d3 from 'd3';

export interface DonutDatum {
  label: string;
  value: number;
  color: string;
}

@Component({
  selector: 'app-d3-donut-chart',
  template: `<div #container class="donut-wrap"><svg #svg></svg></div>`,
  styles: [`
    .donut-wrap { width: 100%; display: flex; justify-content: center; }
    svg { display: block; }
  `],
})
export class D3DonutChartComponent implements OnChanges, OnDestroy {
  data   = input.required<DonutDatum[]>();
  size   = input<number>(130);

  @ViewChild('container') containerRef!: ElementRef<HTMLDivElement>;
  @ViewChild('svg')       svgRef!: ElementRef<SVGSVGElement>;

  private resizeObserver?: ResizeObserver;
  private rendered = false;

  constructor() {
    afterNextRender(() => {
      this.render();
      this.rendered = true;
    });
  }

  ngOnChanges() {
    if (this.rendered) this.render();
  }

  ngOnDestroy() {
    this.resizeObserver?.disconnect();
  }

  render() {
    const svgEl = this.svgRef?.nativeElement;
    if (!svgEl) return;

    const items = this.data();
    if (!items?.length) return;

    const s      = this.size();
    const radius = s / 2;
    const inner  = radius * 0.62;

    d3.select(svgEl).selectAll('*').remove();

    const svg = d3.select(svgEl)
      .attr('width', s)
      .attr('height', s)
      .attr('viewBox', `0 0 ${s} ${s}`);

    const g = svg.append('g')
      .attr('transform', `translate(${radius},${radius})`);

    const pie = d3.pie<DonutDatum>()
      .sort(null)
      .value(d => d.value);

    const arc = d3.arc<d3.PieArcDatum<DonutDatum>>()
      .innerRadius(inner)
      .outerRadius(radius - 2);

    g.selectAll('path')
      .data(pie(items))
      .join('path')
      .attr('d', arc)
      .attr('fill', d => d.data.color)
      .attr('stroke', '#fff')
      .attr('stroke-width', 2);
  }
}
