import { CreateProgrammeDto } from '#types/dto/program.js';
import type { RuleNode } from '@fyp/api/program/types';
import { Body, Controller, Post } from '@nestjs/common';

@Controller('program')
export class ProgramController {
	// WHY I AM SO SMART
	traverseRuleNode(node: RuleNode): boolean {
		const leafNodeCheck: boolean[] = [];
		const childNodeChecks: boolean[] = [];

		if (node.children) {
			for (const child of node.children) {
				if (child.type === 'rule') {
					const nodeCheck = this.traverseRuleNode(child);
					console.log(
						'after traversing child node:',
						child,
						'got:',
						nodeCheck,
					);

					childNodeChecks.push(nodeCheck);
				} else {
					//leaf is a group
					if (child.groupID === '2') {
						leafNodeCheck.push(false);
						console.log('Leaf node:', child, 'fails the check');
					} else {
						leafNodeCheck.push(true);
						console.log('Leaf node:', child, 'passes the check');
					}
				}
			}
		}

		if (childNodeChecks.length > 0) {
			if (node.operator === 'and') {
				const allTrue = childNodeChecks.every(
					(check) => check === true,
				);
				console.log(
					'Node:',
					node,
					'with operator AND has result:',
					allTrue,
				);
				return allTrue;
			} else if (node.operator === 'any') {
				const anyTrue = childNodeChecks.some((check) => check === true);
				console.log(
					'Node:',
					node,
					'with operator ANY has result:',
					anyTrue,
				);
				return anyTrue;
			}
		}

		if (leafNodeCheck.length > 0) {
			if (node.operator === 'and') {
				return leafNodeCheck.every((check) => check === true);
			} else if (node.operator === 'any') {
				return leafNodeCheck.some((check) => check === true);
			}
		}

		return false;
	}

	@Post()
	createProgram(@Body() data: CreateProgrammeDto) {
		console.log(
			'final ans:',
			this.traverseRuleNode(data.categories[0].ruleTree),
		);
	}
}
