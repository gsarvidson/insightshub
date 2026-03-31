import {
  Component, ElementRef, OnChanges, OnDestroy, ViewChild,
  input, afterNextRender,
} from '@angular/core';
import * as d3 from 'd3';

export interface LineSeries {
  label: string;
  color: string;
  data: number[];
  active?: boolean;
}

@Component({
  selector: 'app-d3-line-chart',
  template: `<div #container class="chart-container"><svg #svg></svg></div>`,
  styles: [`
    .chart-container { width: 100%; }
    svg { width: 100%; display: block; }
  `],
})
export class D3LineChartComponent implements OnChanges, OnDestroy {
  series = input.required<LineSeries[]>();
  labels = input.required<string[]>();
  height = input<number>(190);

  @ViewChild('container') containerRef!: ElementRef<HTMLDivElement>;
  @ViewChild('svg')       svgRef!: ElementRef<SVGSVGElement>;

  private resizeObserver?: ResizeObserver;
  private rendered = false;

  constructor() {
    afterNextRender(() => {
      this.setupResizeObserver();
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

  private setupResizeObserver() {
    this.resizeObserver = new ResizeObserver(() => this.render());
    this.resizeObserver.observe(this.containerRef.nativeElement);
  }

  render() {
    const container = this.containerRef?.nativeElement;
    const svgEl = this.svgRef?.nativeElement;
    if (!container || !svgEl) return;

    const allSeries = this.series().filter(s => s.active !== false);
    const lbls = this.labels();
    if (!allSeries.length || !lbls.length) return;

    const width  = container.clientWidth || 500;
    const h      = this.height();
    const margin = { top: 8, right: 16, bottom: 24, left: 32 };
    const innerW = width - margin.left - margin.right;
    const innerH = h - margin.top - margin.bottom;

    d3.select(svgEl).selectAll('*').remove();

    const svg = d3.select(svgEl)
      .attr('viewBox', `0 0 ${width} ${h}`)
      .attr('height', h);

    const g = svg.append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`);

    const x = d3.scalePoint()
      .domain(lbls)
      .range([0, innerW]);

    const allValues = allSeries.flatMap(s => s.data);
    const y = d3.scaleLinear()
      .domain([0, (d3.max(allValues) ?? 1) * 1.1])
      .nice()
      .range([innerH, 0]);

    // Grid lines
    g.append('g')
      .attr('class', 'grid')
      .call(
        d3.axisLeft(y)
          .ticks(4)
          .tickSize(-innerW)
          .tickFormat(() => '')
      )
      .call(ax => ax.select('.domain').remove())
      .selectAll('.tick line')
      .style('stroke', 'var(--color-border-tertiary)')
      .style('stroke-dasharray', '3,3');

    // Axes
    g.append('g')
      .attr('transform', `translate(0,${innerH})`)
      .call(d3.axisBottom(x).tickSize(0))
      .call(ax => ax.select('.domain').remove())
      .selectAll('text')
      .style('font-size', '10px')
      .style('fill', 'var(--color-text-tertiary)');

    g.append('g')
      .call(d3.axisLeft(y).ticks(4))
      .call(ax => ax.select('.domain').remove())
      .selectAll('text')
      .style('font-size', '10px')
      .style('fill', 'var(--color-text-tertiary)');

    // Lines
    const line = d3.line<number>()
      .x((_, i) => x(lbls[i])!)
      .y(d => y(d))
      .curve(d3.curveMonotoneX);

    allSeries.forEach(s => {
      g.append('path')
        .datum(s.data)
        .attr('fill', 'none')
        .attr('stroke', s.color)
        .attr('stroke-width', 2)
        .attr('d', line);
    });
  }
}
